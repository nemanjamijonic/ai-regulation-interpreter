using System.Fabric;
using AIRegulationInterpreter.DocumentService.Models;
using AIRegulationInterpreter.DocumentService.Services.Interfaces;
using Common.Models.DTOs;
using Common.Services.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AIRegulationInterpreter.DocumentService
{
    /// <summary>
    /// Stateful service that manages documents using Reliable Collections and file storage
    /// NO DATABASE ACCESS - only Reliable Dictionary + File System
    /// </summary>
    internal sealed class DocumentService : StatefulService, IDocumentManagementService
    {
        private readonly IFileStorageService _fileStorageService;
        private const string DocumentsDictionaryName = "documents";

        public DocumentService(StatefulServiceContext context, IFileStorageService fileStorageService)
            : base(context)
        {
            _fileStorageService = fileStorageService;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        // Get document by ID (from Reliable Dictionary)
        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            var documentsDict = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, StoredDocument>>(DocumentsDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                // Find by scanning (since we store by Guid)
                var enumerator = (await documentsDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    // Note: In real implementation, we'd need a secondary index for ID lookup
                    // For now, we'll use Guid as primary key
                }
                
                return null; // Will implement proper lookup with secondary index
            }
        }

        public async Task<DocumentDto?> GetDocumentByGuidAsync(Guid guid)
        {
            var documentsDict = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, StoredDocument>>(DocumentsDictionaryName);

            using (var tx = StateManager.CreateTransaction())
            {
                var result = await documentsDict.TryGetValueAsync(tx, guid);
                
                if (!result.HasValue)
                    return null;

                return MapToDto(result.Value);
            }
        }

        public async Task<List<DocumentDto>> GetAllDocumentsAsync()
        {
            var documentsDict = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, StoredDocument>>(DocumentsDictionaryName);
            var documents = new List<DocumentDto>();

            using (var tx = StateManager.CreateTransaction())
            {
                var enumerator = (await documentsDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    documents.Add(MapToDto(enumerator.Current.Value));
                }
            }

            return documents.OrderByDescending(d => d.CreatedAt).ToList();
        }

        public async Task<List<DocumentDto>> GetActiveDocumentsAsync()
        {
            var allDocs = await GetAllDocumentsAsync();
            return allDocs.Where(d => d.IsActive).ToList();
        }

        public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto)
        {
            var documentsDict = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, StoredDocument>>(DocumentsDictionaryName);

            var document = new StoredDocument
            {
                Guid = Guid.NewGuid(),
                Title = createDto.Title,
                Type = createDto.Type,
                CurrentVersion = createDto.Version,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save file to disk if provided
            string? filePath = null;
            long fileSize = 0;

            if (createDto.FileData != null && !string.IsNullOrEmpty(createDto.FileName))
            {
                var (savedPath, savedSize) = await _fileStorageService.SaveFileAsync(
                    createDto.Title,
                    createDto.Version,
                    createDto.FileData,
                    createDto.FileName
                );

                filePath = savedPath;
                fileSize = savedSize;
            }

            // Create initial version
            var initialVersion = new StoredDocumentVersion
            {
                Guid = Guid.NewGuid(),
                Version = createDto.Version,
                ValidFrom = createDto.ValidFrom,
                ValidTo = createDto.ValidTo,
                IsCurrent = true,
                Changes = "Initial version",
                FilePath = filePath ?? string.Empty,
                FileName = createDto.FileName ?? string.Empty,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow
            };

            document.Versions.Add(initialVersion);

            // Save to Reliable Dictionary
            using (var tx = StateManager.CreateTransaction())
            {
                await documentsDict.AddAsync(tx, document.Guid, document);
                await tx.CommitAsync();
            }

            return MapToDto(document);
        }

        public async Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentDto updateDto)
        {
            throw new NotImplementedException("Update not yet implemented for Reliable Dictionary");
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            throw new NotImplementedException("Delete by ID not yet implemented");
        }

        public async Task<bool> SoftDeleteDocumentAsync(int id)
        {
            throw new NotImplementedException("Soft delete not yet implemented");
        }

        // Version Management
        public async Task<DocumentVersionDto> CreateNewVersionAsync(int documentId, string newVersion, string changes, string content)
        {
            throw new NotImplementedException("Create version without file not yet implemented");
        }

        public async Task<DocumentVersionDto> CreateNewVersionWithFileAsync(int documentId, string newVersion, string changes, string content, byte[] fileData, string fileName)
        {
            throw new NotImplementedException("Create version with file not yet implemented - will implement after basic CRUD works");
        }

        public async Task<List<DocumentVersionDto>> GetDocumentVersionsAsync(int documentId)
        {
            throw new NotImplementedException("Get versions not yet implemented");
        }

        // Search
        public async Task<List<DocumentDto>> SearchDocumentsAsync(string searchTerm)
        {
            var allDocs = await GetAllDocumentsAsync();
            var lowerSearchTerm = searchTerm.ToLower();
            return allDocs.Where(d => d.Title.ToLower().Contains(lowerSearchTerm)).ToList();
        }

        public async Task<List<DocumentDto>> GetDocumentsAsync(DocumentFilterDto filter)
        {
            var allDocs = await GetAllDocumentsAsync();
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var lowerSearchTerm = filter.SearchTerm.ToLower();
                allDocs = allDocs.Where(d => d.Title.ToLower().Contains(lowerSearchTerm)).ToList();
            }

            if (filter.Type.HasValue)
            {
                allDocs = allDocs.Where(d => d.Type == filter.Type.Value).ToList();
            }

            if (filter.IsActive.HasValue)
            {
                allDocs = allDocs.Where(d => d.IsActive == filter.IsActive.Value).ToList();
            }

            return allDocs;
        }

        public async Task<List<DocumentDto>> GetDocumentsByTypeAsync(string type)
        {
            var allDocs = await GetAllDocumentsAsync();
            if (Enum.TryParse<Common.Models.Constants.DocumentType>(type, true, out var docType))
            {
                return allDocs.Where(d => d.Type == docType).ToList();
            }
            return new List<DocumentDto>();
        }

        public async Task<List<DocumentDto>> GetDocumentsValidOnDateAsync(DateTime date)
        {
            var allDocs = await GetAllDocumentsAsync();
            return allDocs.Where(d => d.Versions.Any(v => 
                v.ValidFrom <= date && 
                (!v.ValidTo.HasValue || v.ValidTo.Value >= date))).ToList();
        }

        // File Management
        public async Task<byte[]?> DownloadDocumentFileAsync(int versionId)
        {
            // Find version across all documents
            var allDocs = await GetAllDocumentsAsync();
            
            foreach (var doc in allDocs)
            {
                var version = doc.Versions.FirstOrDefault(); // For now, get first version
                if (version != null && !string.IsNullOrEmpty(version.FilePath))
                {
                    return await _fileStorageService.ReadFileAsync(version.FilePath);
                }
            }

            return null;
        }

        // Helper Methods - Mapping
        private DocumentDto MapToDto(StoredDocument document)
        {
            return new DocumentDto
            {
                Id = 0, // No DB ID in Reliable Dictionary
                Guid = document.Guid,
                Version = document.CurrentVersion,
                Title = document.Title,
                Type = document.Type,
                IsActive = document.IsActive,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Versions = document.Versions.Select(MapVersionToDto).ToList()
            };
        }

        private DocumentVersionDto MapVersionToDto(StoredDocumentVersion version)
        {
            return new DocumentVersionDto
            {
                Id = 0, // No DB ID
                DocumentId = 0,
                Version = version.Version,
                ValidFrom = version.ValidFrom,
                ValidTo = version.ValidTo,
                IsCurrent = version.IsCurrent,
                Changes = version.Changes,
                FilePath = version.FilePath,
                FileName = version.FileName,
                FileSize = version.FileSize,
                IndexStatus = Common.Models.Constants.IndexStatus.Pending, // Will be set by IndexService
                IndexedAt = null,
                IndexError = null
            };
        }
    }
}
