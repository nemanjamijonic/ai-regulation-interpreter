using Common.Models.Constants;

namespace Common.Models.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Sections removed - will be in Vector Index only
        public List<DocumentVersionDto> Versions { get; set; } = new List<DocumentVersionDto>();
    }

    public class DocumentVersionDto
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsCurrent { get; set; }
        public string Changes { get; set; } = string.Empty;
        
        // File metadata only (no content)
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        
        // Vector indexing status
        public IndexStatus IndexStatus { get; set; }
        public DateTime? IndexedAt { get; set; }
        public string? IndexError { get; set; }
    }

    public class CreateDocumentDto
    {
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        
        // File upload - NO Sections, NO Content
        public byte[]? FileData { get; set; }
        public string? FileName { get; set; }
    }

    public class UpdateDocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public bool IsActive { get; set; }
    }

    public class DocumentFilterDto
    {
        public string? SearchTerm { get; set; }
        public DocumentType? Type { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ValidOn { get; set; }
    }
}
