using AIRegulationInterpreter.Web.Models;
using Common.Models.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AIRegulationInterpreter.Web.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ILogger<DocumentsController> logger)
        {
            _logger = logger;
        }

        // GET: /Documents
        public IActionResult Index(string? searchTerm, DocumentType? filterType, bool? filterActive)
        {
            // Mock documents data
            var allDocuments = GetMockDocuments();

            // Apply filters
            var filteredDocuments = allDocuments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredDocuments = filteredDocuments.Where(d => 
                    d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (filterType.HasValue)
            {
                filteredDocuments = filteredDocuments.Where(d => d.Type == filterType.Value);
            }

            if (filterActive.HasValue)
            {
                filteredDocuments = filteredDocuments.Where(d => d.IsActive == filterActive.Value);
            }

            var model = new DocumentListViewModel
            {
                Documents = filteredDocuments.ToList(),
                SearchTerm = searchTerm,
                FilterType = filterType,
                FilterActive = filterActive
            };

            return View(model);
        }

        // GET: /Documents/Details/1
        public IActionResult Details(int id)
        {
            // Mock document details
            var model = new DocumentDetailsViewModel
            {
                Id = id,
                Title = "Zakon o zaštiti podataka o li?nosti",
                Type = DocumentType.Zakon,
                Version = "2.1",
                IsActive = true,
                Sections = new List<DocumentSectionViewModel>
                {
                    new DocumentSectionViewModel
                    {
                        Identifier = "?lan 1",
                        Type = "?lan",
                        Content = "Ovim zakonom ure?uju se uslovi za prikupljanje i obradu podataka o li?nosti, prava lica o kojima se podaci obra?uju, ovlaš?enja i obaveze rukovaoca i obra?iva?a podataka.",
                        SubSections = new List<DocumentSectionViewModel>
                        {
                            new DocumentSectionViewModel
                            {
                                Identifier = "?lan 1, Stav 1",
                                Type = "Stav",
                                Content = "Podaci o li?nosti prikupljaju se za odre?ene, zakonite i opravdane svrhe i ne obra?uju se dalje na na?in koji nije u skladu sa tim svrhama."
                            }
                        }
                    },
                    new DocumentSectionViewModel
                    {
                        Identifier = "?lan 15",
                        Type = "?lan",
                        Content = "Bezbednost podataka o li?nosti",
                        SubSections = new List<DocumentSectionViewModel>
                        {
                            new DocumentSectionViewModel
                            {
                                Identifier = "?lan 15, Stav 1",
                                Type = "Stav",
                                Content = "Rukovalac je dužan da preduzme tehni?ke, organizacione i kadrovske mere koje su neophodne radi zaštite podataka o li?nosti."
                            },
                            new DocumentSectionViewModel
                            {
                                Identifier = "?lan 15, Stav 2",
                                Type = "Stav",
                                Content = "Rukovalac je dužan da primenjuje odgovaraju?e tehni?ke, organizacione i kadrovske mere da bi zaštitio podatke o li?nosti od slu?ajnog ili neovlaš?enog uništenja, gubitka, izmene, neovlaš?enog objavljivanja, koriš?enja ili pristupa."
                            }
                        }
                    }
                },
                Versions = new List<DocumentVersionViewModel>
                {
                    new DocumentVersionViewModel
                    {
                        Version = "2.1",
                        ValidFrom = new DateTime(2023, 1, 1),
                        ValidUntil = null,
                        IsCurrent = true,
                        Changes = "Ažurirani ?lanci 15, 18 i 22 u skladu sa EU direktivama"
                    },
                    new DocumentVersionViewModel
                    {
                        Version = "2.0",
                        ValidFrom = new DateTime(2020, 6, 1),
                        ValidUntil = new DateTime(2022, 12, 31),
                        IsCurrent = false,
                        Changes = "Potpuna revizija zakona prema GDPR standardima"
                    },
                    new DocumentVersionViewModel
                    {
                        Version = "1.0",
                        ValidFrom = new DateTime(2018, 1, 1),
                        ValidUntil = new DateTime(2020, 5, 31),
                        IsCurrent = false,
                        Changes = "Inicijalna verzija zakona"
                    }
                }
            };

            return View(model);
        }

        private List<DocumentViewModel> GetMockDocuments()
        {
            return new List<DocumentViewModel>
            {
                new DocumentViewModel
                {
                    Id = 1,
                    Title = "Zakon o zaštiti podataka o li?nosti",
                    Type = DocumentType.Zakon,
                    Version = "2.1",
                    ValidFrom = new DateTime(2023, 1, 1),
                    ValidUntil = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2023, 1, 1)
                },
                new DocumentViewModel
                {
                    Id = 2,
                    Title = "Pravilnik o tehni?kim merama zaštite",
                    Type = DocumentType.Pravilnik,
                    Version = "1.3",
                    ValidFrom = new DateTime(2023, 6, 1),
                    ValidUntil = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2023, 6, 1)
                },
                new DocumentViewModel
                {
                    Id = 3,
                    Title = "Zakon o ra?unovodstvu",
                    Type = DocumentType.Zakon,
                    Version = "3.0",
                    ValidFrom = new DateTime(2022, 1, 1),
                    ValidUntil = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2022, 1, 1)
                },
                new DocumentViewModel
                {
                    Id = 4,
                    Title = "Interna politika zaštite podataka",
                    Type = DocumentType.InternaPolitika,
                    Version = "1.0",
                    ValidFrom = new DateTime(2024, 1, 1),
                    ValidUntil = null,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new DocumentViewModel
                {
                    Id = 5,
                    Title = "Zakon o elektronskoj trgovini (zastario)",
                    Type = DocumentType.Zakon,
                    Version = "1.0",
                    ValidFrom = new DateTime(2015, 1, 1),
                    ValidUntil = new DateTime(2023, 12, 31),
                    IsActive = false,
                    CreatedAt = new DateTime(2015, 1, 1)
                }
            };
        }
    }
}
