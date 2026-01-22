using Common.Context;
using Common.Models.Constants;
using Common.Models.DbModels;
using Common.Models.DTOs;
using Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace AIRegulationInterpreter.Web.Controllers.Api
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsApiController : ControllerBase
    {
        private readonly AIRegulationInterpreterContext _context;
        private readonly IDocumentManagementService _documentServiceProxy;
        private readonly ILogger<DocumentsApiController> _logger;

        public DocumentsApiController(AIRegulationInterpreterContext context, ILogger<DocumentsApiController> logger)
        {
            _context = context;
            _logger = logger;
            
            // Create proxy to DocumentService for file operations
            ServicePartitionKey partition = new ServicePartitionKey(long.Parse("1"));
            _documentServiceProxy = ServiceProxy.Create<IDocumentManagementService>(
                new Uri("fabric:/AIRegulationInterpreter/AIRegulationInterpreter.DocumentService"),
                partition
            );
        }

        /// <summary>
        /// Get all documents
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DocumentDto>>> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Versions)
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return Ok(documents.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Versions.OrderByDescending(v => v.ValidFrom))
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (document == null)
                    return NotFound($"Document with ID {id} not found");

                return Ok(MapToDto(document));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload a new document with PDF file
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            try
            {
                _logger.LogInformation("?? Upload started - Title: {Title}, Version: {Version}", request.Title, request.Version);
                
                if (request.File == null || request.File.Length == 0)
                    return BadRequest("File is required");

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
                var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

                // Convert file to byte array
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await request.File.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                _logger.LogInformation("? File loaded - Size: {Size} bytes", fileData.Length);

                // 1. FIRST: Save to SQL Database
                var document = new Document
                {
                    Guid = Guid.NewGuid(),
                    Title = request.Title,
                    Type = request.Type,
                    Version = request.Version,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var initialVersion = new DocumentVersion
                {
                    Guid = Guid.NewGuid(),
                    Version = request.Version,
                    ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
                    ValidTo = request.ValidTo,
                    IsCurrent = true,
                    Changes = "Initial version",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IndexStatus = IndexStatus.Pending // Will be indexed by background job
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                document.Versions = new List<DocumentVersion> { initialVersion };

                await _context.SaveChangesAsync();

                _logger.LogInformation("?? Saving to database...");
                _logger.LogInformation("? Database save successful - Document ID: {Id}", document.Id);

                // 2. THEN: Call DocumentService to save file + metadata
                try
                {
                    _logger.LogInformation("?? Calling DocumentService to save file...");
                    
                    var createDto = new CreateDocumentDto
                    {
                        Title = request.Title,
                        Type = request.Type,
                        Version = request.Version,
                        ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
                        ValidTo = request.ValidTo,
                        FileData = fileData,
                        FileName = request.File.FileName
                    };

                    var docServiceResult = await _documentServiceProxy.CreateDocumentAsync(createDto);
                    _logger.LogInformation("? DocumentService call successful");

                    // Update DB with file metadata
                    if (docServiceResult.Versions.Any())
                    {
                        var fileVersion = docServiceResult.Versions.First();
                        initialVersion.FilePath = fileVersion.FilePath;
                        initialVersion.FileName = fileVersion.FileName;
                        initialVersion.FileSize = fileVersion.FileSize;

                        _logger.LogInformation("?? Updating DB with file metadata - Path: {Path}", fileVersion.FilePath);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("? Database update successful");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error calling DocumentService (file may not be saved)");
                    // Don't fail - we have DB record
                }

                // Reload document with all navigation properties
                var createdDocument = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == document.Id);

                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, MapToDto(createdDocument!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error uploading document");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Download document file by version ID
        /// </summary>
        [HttpGet("download/{versionId}")]
        public async Task<ActionResult> DownloadDocument(int versionId)
        {
            try
            {
                var fileData = await _documentServiceProxy.DownloadDocumentFileAsync(versionId);
                
                if (fileData == null)
                    return NotFound($"File for version {versionId} not found");

                // Get version info from DB
                var version = await _context.DocumentVersions
                    .FirstOrDefaultAsync(v => v.Id == versionId);

                var fileName = version?.FileName ?? "document.pdf";
                var contentType = GetContentType(fileName);

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document version {VersionId}", versionId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search documents
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<DocumentDto>>> SearchDocuments([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest("Search term is required");

                var lowerSearchTerm = searchTerm.ToLower();
                var documents = await _context.Documents
                    .Include(d => d.Versions)
                    .Where(d => !d.IsDeleted && d.Title.ToLower().Contains(lowerSearchTerm))
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return Ok(documents.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get active documents
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<DocumentDto>>> GetActiveDocuments()
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Versions)
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return Ok(documents.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active documents");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get document versions
        /// </summary>
        [HttpGet("{documentId}/versions")]
        public async Task<ActionResult<List<DocumentVersionDto>>> GetDocumentVersions(int documentId)
        {
            try
            {
                var versions = await _context.DocumentVersions
                    .Where(v => v.DocumentId == documentId && !v.IsDeleted)
                    .OrderByDescending(v => v.ValidFrom)
                    .ToListAsync();

                return Ok(versions.Select(MapVersionToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document versions for {DocumentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }

        // Helper methods
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };
        }

        private DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                Guid = document.Guid,
                Version = document.Version,
                Title = document.Title,
                Type = document.Type,
                IsActive = document.IsActive,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Versions = document.Versions?.Select(MapVersionToDto).ToList() ?? new List<DocumentVersionDto>()
            };
        }

        private DocumentVersionDto MapVersionToDto(DocumentVersion version)
        {
            return new DocumentVersionDto
            {
                Id = version.Id,
                DocumentId = version.DocumentId,
                Version = version.Version,
                ValidFrom = version.ValidFrom,
                ValidTo = version.ValidTo,
                IsCurrent = version.IsCurrent,
                Changes = version.Changes,
                FilePath = version.FilePath,
                FileName = version.FileName,
                FileSize = version.FileSize,
                IndexStatus = version.IndexStatus,
                IndexedAt = version.IndexedAt,
                IndexError = version.IndexError
            };
        }
    }

    /// <summary>
    /// Request model for uploading documents
    /// </summary>
    public class UploadDocumentRequest
    {
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string Version { get; set; } = "1.0";
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
