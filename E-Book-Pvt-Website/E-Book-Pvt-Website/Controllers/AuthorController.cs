using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Book_Pvt_Website.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AuthorList()
        {
            var authors = _context.Author.ToList(); 
            return View(authors);
        }

        public IActionResult CreateAuthor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuthor(Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Author.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction("AuthorList");
            }

            return View(author);
        }
    }
}
