using System.Diagnostics;
using AIRegulationInterpreter.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIRegulationInterpreter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Quick search redirect from homepage
        [HttpGet]
        public IActionResult QuickSearch(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return RedirectToAction("Ask", "Query");
            }

            return RedirectToAction("Ask", "Query", new { question });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
