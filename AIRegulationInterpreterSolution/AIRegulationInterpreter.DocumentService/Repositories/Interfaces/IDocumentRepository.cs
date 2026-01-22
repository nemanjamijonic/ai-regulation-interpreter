using Common.Models.DbModels;
using Common.Models.DTOs;

namespace AIRegulationInterpreter.DocumentService.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        // Document CRUD
        Task<Document?> GetByIdAsync(int id);
        Task<Document?> GetByGuidAsync(Guid guid);
        Task<List<Document>> GetAllAsync();
        Task<List<Document>> GetActiveAsync();
        Task<List<Document>> GetByTypeAsync(string type);
        Task<List<Document>> GetValidOnDateAsync(DateTime date);
        Task<List<Document>> SearchAsync(string searchTerm);
        Task<List<Document>> GetFilteredAsync(DocumentFilterDto filter);
        Task<Document> CreateAsync(Document document);
        Task<Document> UpdateAsync(Document document);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Sections - REMOVED (will be in Vector Index)
        // Task<List<DocumentSection>> GetSectionsAsync(int documentId);
        // Task<DocumentSection?> GetSectionByIdAsync(int sectionId);
        // Task<List<DocumentSection>> CreateSectionsAsync(List<DocumentSection> sections);

        // Versions
        Task<List<DocumentVersion>> GetVersionsAsync(int documentId);
        Task<DocumentVersion> CreateVersionAsync(DocumentVersion version);
    }
}
