using Common.Models.Constants;

namespace Common.Models.DbModels
{
    public class DocumentVersion : BaseModel
    {
        public int DocumentId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsCurrent { get; set; }
        public string Changes { get; set; } = string.Empty; // Description of changes
        
        // File metadata ONLY (no content!)
        public string? FilePath { get; set; } // Physical file path (PDF, DOCX, etc.)
        public string? FileName { get; set; } // Original file name
        public long? FileSize { get; set; } // File size in bytes
        
        // Vector indexing status
        public IndexStatus IndexStatus { get; set; } = IndexStatus.Pending;
        public DateTime? IndexedAt { get; set; }
        public string? IndexError { get; set; } // Error message if indexing failed

        // Navigation property
        public virtual Document Document { get; set; } = null!;
    }
}
