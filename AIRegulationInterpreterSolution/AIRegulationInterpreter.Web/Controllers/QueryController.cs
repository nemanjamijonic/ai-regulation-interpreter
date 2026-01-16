using AIRegulationInterpreter.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIRegulationInterpreter.Web.Controllers
{
    public class QueryController : Controller
    {
        private readonly ILogger<QueryController> _logger;

        public QueryController(ILogger<QueryController> logger)
        {
            _logger = logger;
        }

        // GET: /Query/Ask
        public IActionResult Ask(string? question)
        {
            var model = new QueryViewModel();
            if (!string.IsNullOrWhiteSpace(question))
            {
                model.Question = question;
            }
            return View(model);
        }

        // POST: /Query/Ask
        [HttpPost]
        public IActionResult Ask(QueryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mock response - kasnije ?e ovo pozivati QueryService API
            var result = new QueryResultViewModel
            {
                Question = model.Question,
                Answer = "Na osnovu analize relevantnih propisa, možemo zaklju?iti da je odgovor slede?i...",
                Explanation = "Prema ?lanu 15. stav 2. Zakona o zaštiti podataka o li?nosti, organizacija je dužna da obezbedi odgovaraju?e tehni?ke i organizacione mere zaštite podataka. To zna?i da morate implementirati enkripciju, kontrolu pristupa i redovne sigurnosne provere.",
                ConfidenceLevel = 0.87,
                Timestamp = DateTime.Now,
                References = new List<DocumentReferenceViewModel>
                {
                    new DocumentReferenceViewModel
                    {
                        DocumentTitle = "Zakon o zaštiti podataka o li?nosti",
                        DocumentType = "Zakon",
                        Section = "?lan 15, Stav 2",
                        ArticleNumber = "?lan 15",
                        Citation = "Rukovalac je dužan da primenjuje odgovaraju?e tehni?ke, organizacione i kadrovske mere da bi zaštitio podatke o li?nosti od slu?ajnog ili neovlaš?enog uništenja, gubitka, izmene, neovlaš?enog objavljivanja..."
                    },
                    new DocumentReferenceViewModel
                    {
                        DocumentTitle = "Pravilnik o tehni?kim merama zaštite",
                        DocumentType = "Pravilnik",
                        Section = "?lan 8, Stav 1",
                        ArticleNumber = "?lan 8",
                        Citation = "Tehni?ke mere zaštite obuhvataju primenu kriptografskih metoda, kontrolu pristupa informacionim sistemima i redovne provere bezbednosti."
                    }
                }
            };

            return View("Result", result);
        }

        // GET: /Query/History
        public IActionResult History()
        {
            // Mock history data
            var model = new QueryHistoryViewModel
            {
                Queries = new List<QueryHistoryItemViewModel>
                {
                    new QueryHistoryItemViewModel
                    {
                        Id = 1,
                        Question = "Koje su obaveze organizacije u vezi sa zaštitom li?nih podataka?",
                        ShortAnswer = "Organizacija mora primeniti odgovaraju?e tehni?ke i organizacione mere...",
                        ConfidenceLevel = 0.87,
                        Timestamp = DateTime.Now.AddHours(-2)
                    },
                    new QueryHistoryItemViewModel
                    {
                        Id = 2,
                        Question = "Koji su uslovi za objavljivanje finansijskih izveštaja?",
                        ShortAnswer = "Prema ?lanu 42. Zakona o ra?unovodstvu, pravna lica su dužna...",
                        ConfidenceLevel = 0.92,
                        Timestamp = DateTime.Now.AddDays(-1)
                    },
                    new QueryHistoryItemViewModel
                    {
                        Id = 3,
                        Question = "Da li je dozvoljeno snimanje razgovora sa klijentima?",
                        ShortAnswer = "Snimanje je dozvoljeno uz prethodnu saglasnost klijenta...",
                        ConfidenceLevel = 0.78,
                        Timestamp = DateTime.Now.AddDays(-3)
                    }
                }
            };

            return View(model);
        }

        // GET: /Query/Details/1
        public IActionResult Details(int id)
        {
            // Mock detailed result
            var model = new QueryResultViewModel
            {
                Question = "Koje su obaveze organizacije u vezi sa zaštitom li?nih podataka?",
                Answer = "Na osnovu analize relevantnih propisa, možemo zaklju?iti da je odgovor slede?i...",
                Explanation = "Prema ?lanu 15. stav 2. Zakona o zaštiti podataka o li?nosti, organizacija je dužna da obezbedi odgovaraju?e tehni?ke i organizacione mere zaštite podataka. To zna?i da morate implementirati enkripciju, kontrolu pristupa i redovne sigurnosne provere.",
                ConfidenceLevel = 0.87,
                Timestamp = DateTime.Now.AddHours(-2),
                References = new List<DocumentReferenceViewModel>
                {
                    new DocumentReferenceViewModel
                    {
                        DocumentTitle = "Zakon o zaštiti podataka o li?nosti",
                        DocumentType = "Zakon",
                        Section = "?lan 15, Stav 2",
                        ArticleNumber = "?lan 15",
                        Citation = "Rukovalac je dužan da primenjuje odgovaraju?e tehni?ke, organizacione i kadrovske mere da bi zaštitio podatke o li?nosti od slu?ajnog ili neovlaš?enog uništenja, gubitka, izmene, neovlaš?enog objavljivanja..."
                    }
                }
            };

            return View("Result", model);
        }
    }
}
