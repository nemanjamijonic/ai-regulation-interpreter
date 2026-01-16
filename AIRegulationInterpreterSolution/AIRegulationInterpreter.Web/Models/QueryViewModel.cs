namespace AIRegulationInterpreter.Web.Models
{
    public class QueryViewModel
    {
        public string Question { get; set; } = string.Empty;
        public DateTime? ContextDate { get; set; }
        public string? OrganizationType { get; set; }
        public List<string> DocumentTypes { get; set; } = new List<string>();
    }

    public class QueryResultViewModel
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
        public List<DocumentReferenceViewModel> References { get; set; } = new List<DocumentReferenceViewModel>();
        public DateTime Timestamp { get; set; }
    }

    public class DocumentReferenceViewModel
    {
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Citation { get; set; } = string.Empty;
        public string ArticleNumber { get; set; } = string.Empty;
    }

    public class QueryHistoryViewModel
    {
        public List<QueryHistoryItemViewModel> Queries { get; set; } = new List<QueryHistoryItemViewModel>();
    }

    public class QueryHistoryItemViewModel
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string ShortAnswer { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
