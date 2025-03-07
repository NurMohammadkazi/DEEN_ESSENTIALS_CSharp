using System.Diagnostics;
using Deen_Essentials.Models;
using Deen_Essentials.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Deen_Essentials.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDatabaseContext _context;
        public HomeController(ILogger<HomeController> logger, MyDatabaseContext context)
        {
            _logger = logger;

            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList(); // Fetch all products
            return View(products); // Pass the products to the view
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
