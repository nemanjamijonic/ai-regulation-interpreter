using Common.Models.DTOs;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Services.Interfaces
{
    public interface IDocumentManagementService : IService
    {
        // CRUD Operations
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<DocumentDto?> GetDocumentByGuidAsync(Guid guid);
        Task<List<DocumentDto>> GetAllDocumentsAsync();
        Task<List<DocumentDto>> GetDocumentsAsync(DocumentFilterDto filter);
        Task<List<DocumentDto>> GetActiveDocumentsAsync();
        Task<List<DocumentDto>> GetDocumentsByTypeAsync(string type);
        Task<List<DocumentDto>> GetDocumentsValidOnDateAsync(DateTime date);
        Task<List<DocumentDto>> SearchDocumentsAsync(string searchTerm);
        Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto);
        Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentDto updateDto);
        Task<bool> DeleteDocumentAsync(int id);
        Task<bool> SoftDeleteDocumentAsync(int id);

        // Version Management
        Task<DocumentVersionDto> CreateNewVersionAsync(int documentId, string newVersion, string changes, string content);
        Task<DocumentVersionDto> CreateNewVersionWithFileAsync(int documentId, string newVersion, string changes, string content, byte[] fileData, string fileName);
        Task<List<DocumentVersionDto>> GetDocumentVersionsAsync(int documentId);

        // Section Management - REMOVED (will be in Vector Index)
        // Task<List<DocumentSectionDto>> GetDocumentSectionsAsync(int documentId);
        // Task<DocumentSectionDto?> GetSectionByIdAsync(int sectionId);

        // File Management
        Task<byte[]?> DownloadDocumentFileAsync(int versionId);
    }
}
