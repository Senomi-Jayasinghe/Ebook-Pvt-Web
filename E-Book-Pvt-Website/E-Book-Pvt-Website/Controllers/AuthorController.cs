using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult EditAuthor(int id)
        {
            // Get the author to edit
            var author = _context.Author.FirstOrDefault(b => b.author_id == id);
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAuthor(int id, Author author)
        {
            if (id != author.author_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing author record
                    var existingAuthor = _context.Author.FirstOrDefault(b => b.author_id == id);
                    if (existingAuthor == null)
                    {
                        return NotFound();
                    }

                    // Update author details
                    existingAuthor.author_name = author.author_name;

                    // Save changes to the database
                    _context.Update(existingAuthor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Author.Any(b => b.author_id == author.author_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AuthorList)); // Redirect to the list of authors
            }
            return View(author);
        }

        [HttpGet]
        public IActionResult DeleteAuthor(int id)
        {
            var author = _context.Author.FirstOrDefault(b => b.author_id == id);
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuthorConfirmed([FromForm] int author_id)
        {
            var author = await _context.Author.FindAsync(author_id);

            if (author == null)
            {
                return NotFound(); // Return 404 if author not found
            }

            _context.Author.Remove(author);

            // Save changes to apply the deletion
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AuthorList));
        }
    }
}
