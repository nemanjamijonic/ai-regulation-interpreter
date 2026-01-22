using Common.Models.Constants;

namespace Common.Models.DbModels
{
    public class Document : BaseModel
    {
        public string Version { get; set; } = string.Empty; // Trenutna verzija (denormalizovano)
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties - SAMO Versions (bez Sections)
        public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    }
}
