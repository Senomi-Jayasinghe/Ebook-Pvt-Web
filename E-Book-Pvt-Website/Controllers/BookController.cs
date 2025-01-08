using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Helpers;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Books()
        {
            var books = await _context.Book.ToListAsync();

            // Fetch authors as a dictionary of author IDs to names
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);

            // Pass the dictionary to the view using ViewBag
            ViewBag.AuthorNames = authors;

            return View(books);
        }

        public IActionResult AddBooks()
        {
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "category_id", "category_name");

            var authors = _context.Author.ToList();
            ViewBag.AuthorList = new SelectList(authors, "author_id", "author_name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBooks(Book book, IFormFile bookImage, int? selectedCategoryId)
        {
            ModelState.Remove("book_image");
            // Check ModelState errors
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine("ModelState Error: " + error.ErrorMessage); // Or use logging
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (bookImage != null && bookImage.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await bookImage.CopyToAsync(memoryStream);
                        book.book_image = memoryStream.ToArray();
                    }
                }

                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction("Books");
            }

            return View(book);
        }


        public async Task<IActionResult> BookDetails(int id)
        {
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);
            if (book == null)
            {
                return NotFound();
            }

            var authors = _context.Author.ToDictionary(a => a.author_id, a => a.author_name);

            // Pass authors to the view
            ViewBag.AuthorNames = authors;

            // Convert the image to a base64 string if it exists
            if (book.book_image != null)
            {
                var base64Image = Convert.ToBase64String(book.book_image);
                ViewData["ImageBase64"] = $"data:image/jpeg;base64,{base64Image}";
            }

            return View(book);
        }

        // GET: EditBooks
        public IActionResult EditBooks(int id)
        {
            // Get the list of authors from the database
            var authors = _context.Author
                                  .Select(a => new { a.author_id, a.author_name })
                                  .ToList();

            // Create the SelectList for dropdown options
            ViewBag.AuthorNames = new SelectList(authors, "author_id", "author_name");

            // Get the book to edit
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);

            // Convert the book image to a base64 string if it exists
            if (book?.book_image != null)
            {
                ViewBag.ImageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(book.book_image)}";
            }

            return View(book);
        }


        // POST: EditBooks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooks(int id, Book book, IFormFile bookImage)
        {
            if (id != book.book_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing book record
                    var existingBook = _context.Book.FirstOrDefault(b => b.book_id == id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    // Update book details
                    existingBook.book_title = book.book_title;
                    existingBook.book_description = book.book_description;
                    existingBook.book_publisher = book.book_publisher;
                    existingBook.book_price = book.book_price;
                    existingBook.book_ISBN = book.book_ISBN;
                    existingBook.book_author_id = book.book_author_id;

                    // If a new image is uploaded, replace the existing one
                    if (bookImage != null && bookImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await bookImage.CopyToAsync(memoryStream);
                            existingBook.book_image = memoryStream.ToArray(); // Store image as byte array
                        }
                    }

                    // Save changes to the database
                    _context.Update(existingBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Book.Any(b => b.book_id == book.book_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Books)); // Redirect to the list of books after editing
            }

            // If form submission fails, repopulate the SelectList for authors
            var authors = _context.Author
                                  .Select(a => new { a.author_id, a.author_name })
                                  .ToList();
            ViewBag.AuthorNames = new SelectList(authors, "author_id", "author_name");

            // Pass the current image back to the view if there was an error
            if (book.book_image != null)
            {
                ViewBag.ImageBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(book.book_image)}";
            }

            return View(book);
        }

        public async Task<IActionResult> BrowseBooks(string searchTitle, int? categoryId)
        {
            // Fetch and filter books
            var booksQuery = _context.Book.AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                booksQuery = booksQuery.Where(b => b.book_title.Contains(searchTitle));
            }

            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => _context.BookCategory
                    .Any(bc => bc.book_id == b.book_id && bc.category_id == categoryId));
            }

            var books = await booksQuery.ToListAsync();

            // Pass current search values to ViewBag
            ViewBag.SearchTitle = searchTitle;
            ViewBag.CategoryId = categoryId;

            // Fetch authors and categories
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);
            var categories = await _context.Category.ToListAsync();

            ViewBag.AuthorNames = authors;
            ViewBag.CategoryList = new SelectList(categories, "category_id", "category_name");

            // Prepare image URLs in the ViewBag for each book
            ViewBag.ImageUrls = books.ToDictionary(
                b => b.book_id,
                b => b.book_image != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(b.book_image)}"
                    : "fallback.jpg"
            );

            return View(books);
        }

        public async Task<IActionResult> BrowseDetails(int id)
        {
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);
            if (book == null)
            {
                return NotFound();
            }

            var authors = _context.Author.ToDictionary(a => a.author_id, a => a.author_name);

            // Pass authors to the view
            ViewBag.AuthorNames = authors;

            // Fetch feedback for the book
            var feedbackList = _context.BookFeedback
                                        .Where(f => f.book_id == id)
                                        .Select(f => f.feedback)
                                        .ToList();

            ViewBag.FeedbackList = feedbackList;

            // Convert the image to a base64 string if it exists
            if (book.book_image != null)
            {
                var base64Image = Convert.ToBase64String(book.book_image);
                ViewData["ImageBase64"] = $"data:image/jpeg;base64,{base64Image}";
            }

            return View(book);
        }

        [HttpPost]
        public IActionResult AddFeedback(int bookId, string feedbackText)
        {
            int? customerId = HttpContext.Session.GetInt32("customer_id");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            // Create a new BookFeedback entry
            var feedback = new BookFeedback
            {
                customer_id = customerId.Value,
                book_id = bookId,
                feedback = feedbackText
            };

            // Save the feedback to the database
            _context.BookFeedback.Add(feedback);
            _context.SaveChanges();

            // Redirect back to the book details page
            return RedirectToAction("BrowseDetails", new { id = bookId });
        }

        public IActionResult AddToCart(int bookId, int quantity)
        {
            // Fetch the book details from the database
            var book = _context.Book.FirstOrDefault(b => b.book_id == bookId);
            if (book == null)
            {
                return NotFound();
            }

            // Check if the quantity is valid (don't exceed book_quantity in the database)
            if (quantity > book.book_quantity)
            {
                TempData["Message"] = $"Only {book.book_quantity} books available in stock.";
                return RedirectToAction("BrowseDetails", new { id = bookId });
            }

            // Retrieve the cart from session storage or create a new one
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Check if the book is already in the cart
            var existingCartItem = cart.FirstOrDefault(ci => ci.Book.book_id == bookId);
            if (existingCartItem != null)
            {
                // Update the quantity of the existing book in the cart
                existingCartItem.Quantity += quantity;
                // Ensure the quantity does not exceed available stock
                if (existingCartItem.Quantity > book.book_quantity)
                {
                    existingCartItem.Quantity = book.book_quantity; // Set to max available quantity
                    TempData["Message"] = $"Only {book.book_quantity} books available in stock.";
                }
            }
            else
            {
                // Add new book with quantity to the cart
                cart.Add(new CartItem
                {
                    Book = book,
                    Quantity = quantity
                });
            }

            // Save the updated cart to session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ShoppingCart()
        {
            // Retrieve the cart items from the session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            return View(cart); // Pass the correct model type (List<CartItem>) to the view
        }


        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, string action)
        {
            // Fetch the book details from the database
            var book = _context.Book.FirstOrDefault(b => b.book_id == bookId);
            if (book == null)
            {
                return NotFound();
            }

            // Retrieve the cart from session storage
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Find the cart item for the book
            var cartItem = cart.FirstOrDefault(ci => ci.Book.book_id == bookId);
            if (cartItem != null)
            {
                if (action == "add")
                {
                    // Increase quantity, but ensure it doesn't exceed available stock
                    if (cartItem.Quantity < book.book_quantity)
                    {
                        cartItem.Quantity++;
                    }
                }
                else if (action == "minus")
                {
                    // Decrease quantity, but not below 1
                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                    }
                }
            }

            // Save the updated cart to session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("ShoppingCart");
        }


        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = _context.Book.FirstOrDefault(b => b.book_id == id);
            if (book == null)
            {
                return NotFound();
            }

            var authors = _context.Author.ToDictionary(a => a.author_id, a => a.author_name);

            // Pass authors to the view
            ViewBag.AuthorNames = authors;

            // Convert the image to a base64 string if it exists
            if (book.book_image != null)
            {
                var base64Image = Convert.ToBase64String(book.book_image);
                ViewData["ImageBase64"] = $"data:image/jpeg;base64,{base64Image}";
            }

            return View(book);
        }
    }
}
