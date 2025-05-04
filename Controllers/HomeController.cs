using System.Diagnostics;
using EmmettPierson.com.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmmettPierson.com.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly LedgerContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, LedgerContext db)
        {
            _logger = logger;
            _context = db;
            db.Database.EnsureCreated();
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Bio")]
        public IActionResult Bio()
        {
            return View();
        }

        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("Portfolio")]
        public IActionResult Portfolio()
        {
            return View();
        }

        [Route("Resume")]
        public IActionResult Resume()
        {
            return View();
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
