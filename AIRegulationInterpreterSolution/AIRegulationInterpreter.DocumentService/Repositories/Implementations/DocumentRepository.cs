using AIRegulationInterpreter.DocumentService.Repositories.Interfaces;
using Common.Context;
using Common.Models.DbModels;
using Common.Models.DTOs;
using Common.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace AIRegulationInterpreter.DocumentService.Repositories.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        protected readonly AIRegulationInterpreterContext _context;

        public DocumentRepository(AIRegulationInterpreterContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Versions.OrderByDescending(v => v.ValidFrom))
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<Document?> GetByGuidAsync(Guid guid)
        {
            return await _context.Documents
                .Include(d => d.Versions.OrderByDescending(v => v.ValidFrom))
                .FirstOrDefaultAsync(d => d.Guid == guid && !d.IsDeleted);
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetActiveAsync()
        {
            return await _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted && d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByTypeAsync(string type)
        {
            if (!Enum.TryParse<DocumentType>(type, true, out var docType))
            {
                return new List<Document>();
            }

            return await _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted && d.Type == docType)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetValidOnDateAsync(DateTime date)
        {
            return await _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted && d.Versions.Any(v => 
                    v.ValidFrom <= date && 
                    (!v.ValidTo.HasValue || v.ValidTo.Value >= date)))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> SearchAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted && d.Title.ToLower().Contains(lowerSearchTerm))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Document>> GetFilteredAsync(DocumentFilterDto filter)
        {
            var query = _context.Documents
                .Include(d => d.Versions)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var lowerSearchTerm = filter.SearchTerm.ToLower();
                query = query.Where(d => d.Title.ToLower().Contains(lowerSearchTerm));
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(d => d.Type == filter.Type.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == filter.IsActive.Value);
            }

            if (filter.ValidOn.HasValue)
            {
                var date = filter.ValidOn.Value;
                query = query.Where(d => d.Versions.Any(v => 
                    v.ValidFrom <= date && 
                    (!v.ValidTo.HasValue || v.ValidTo.Value >= date)));
            }

            return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        }

        public async Task<Document> CreateAsync(Document document)
        {
            document.Guid = Guid.NewGuid();
            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            document.IsDeleted = false;

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            
            // Load navigation properties
            await _context.Entry(document)
                .Collection(d => d.Versions)
                .LoadAsync();

            return document;
        }

        public async Task<Document> UpdateAsync(Document document)
        {
            document.UpdatedAt = DateTime.UtcNow;
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            document.IsDeleted = true;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Documents.AnyAsync(d => d.Id == id && !d.IsDeleted);
        }

        // Versions
        public async Task<List<DocumentVersion>> GetVersionsAsync(int documentId)
        {
            return await _context.DocumentVersions
                .Where(v => v.DocumentId == documentId && !v.IsDeleted)
                .OrderByDescending(v => v.ValidFrom)
                .ToListAsync();
        }

        public async Task<DocumentVersion> CreateVersionAsync(DocumentVersion version)
        {
            version.Guid = Guid.NewGuid();
            version.CreatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;
            version.IsDeleted = false;

            _context.DocumentVersions.Add(version);
            await _context.SaveChangesAsync();
            return version;
        }
    }
}
