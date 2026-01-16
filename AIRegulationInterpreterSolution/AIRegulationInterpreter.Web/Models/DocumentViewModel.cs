namespace AIRegulationInterpreter.Web.Models
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DocumentDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public List<DocumentSectionViewModel> Sections { get; set; } = new List<DocumentSectionViewModel>();
        public List<DocumentVersionViewModel> Versions { get; set; } = new List<DocumentVersionViewModel>();
    }

    public class DocumentSectionViewModel
    {
        public string Identifier { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // ?lan, Stav, Ta?ka
        public string Content { get; set; } = string.Empty;
        public List<DocumentSectionViewModel> SubSections { get; set; } = new List<DocumentSectionViewModel>();
    }

    public class DocumentVersionViewModel
    {
        public string Version { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsCurrent { get; set; }
        public string Changes { get; set; } = string.Empty;
    }

    public class DocumentListViewModel
    {
        public List<DocumentViewModel> Documents { get; set; } = new List<DocumentViewModel>();
        public string? SearchTerm { get; set; }
        public string? FilterType { get; set; }
        public bool? FilterActive { get; set; }
    }
}
