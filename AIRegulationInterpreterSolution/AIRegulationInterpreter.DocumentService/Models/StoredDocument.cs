using Common.Models.Constants;

namespace AIRegulationInterpreter.DocumentService.Models
{
    /// <summary>
    /// Lightweight document model for Reliable Dictionary storage
    /// </summary>
    [Serializable]
    public class StoredDocument
    {
        public Guid Guid { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // File storage info
        public List<StoredDocumentVersion> Versions { get; set; } = new List<StoredDocumentVersion>();
    }

    [Serializable]
    public class StoredDocumentVersion
    {
        public Guid Guid { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsCurrent { get; set; }
        public string Changes { get; set; } = string.Empty;
        
        // File metadata
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
