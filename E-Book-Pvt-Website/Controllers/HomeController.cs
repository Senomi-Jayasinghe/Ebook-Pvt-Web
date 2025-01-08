using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace E_Book_Pvt_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult CustomerLogin()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult Books()
        {
            return View();
        }

        public IActionResult AdminList()
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
