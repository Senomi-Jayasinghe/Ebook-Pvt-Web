using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Reflection.Metadata.BlobBuilder;
using System.Text.Json; // Ensure System.Text.Json is imported for Json serialization

using ClosedXML.Excel; // For Excel generation

using System.Linq;
using DocumentFormat.OpenXml.InkML;

namespace E_Book_Pvt_Website.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminList()
        {
            var admins = await _context.Admin.ToListAsync();
            return View(admins);
        }

        public IActionResult CreateAdmin()
        {
            return View(new Admin());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.Admin.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction("AdminList");
            }

            return View(admin);
        }

        public IActionResult EditAdmin(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(int id, Admin admin)
        {
            if (id != admin.admin_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing admin record
                    var existingAdmin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
                    if (existingAdmin == null)
                    {
                        return NotFound();
                    }

                    // Update admin details
                    existingAdmin.admin_name = admin.admin_name;
                    existingAdmin.admin_phoneno = admin.admin_phoneno;
                    existingAdmin.admin_email = admin.admin_email;
                    existingAdmin.admin_password = admin.admin_password;

                    // Save changes to the database
                    _context.Update(existingAdmin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Admin.Any(b => b.admin_id == admin.admin_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AdminList)); // Redirect to the list of books after editing
            }
            return View(admin);
        }

        public IActionResult AdminDetails(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpGet]
        public IActionResult DeleteAdmin(int id)
        {
            // Get the admin to edit
            var admin = _context.Admin.FirstOrDefault(b => b.admin_id == id);
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdminConfirmed(int id)
        {
            // Retrieve the admin by admin_id
            var admin = await _context.Admin.FindAsync(id);

            if (admin == null)
            {
                return NotFound(); // Return 404 if admin not found
            }

            // Delete the admin from the database
            _context.Admin.Remove(admin);

            // Save changes to apply the deletion
            await _context.SaveChangesAsync();

            // Redirect to the admin list page after deletion
            return RedirectToAction(nameof(AdminList));
        }



        public async Task<IActionResult> AdminDashboard()
        {
            // Orders summary by status for pie chart
            var orderStatuses = await _context.Order
                .GroupBy(o => o.order_status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Prepare data for the chart
            ViewBag.StatusLabels = JsonSerializer.Serialize(orderStatuses.Select(s => s.Status).ToList());
            ViewBag.StatusData = JsonSerializer.Serialize(orderStatuses.Select(s => s.Count).ToList());

            // Total number of orders placed
            ViewBag.TotalOrders = await _context.Order.CountAsync();

            // Total number of books
            ViewBag.TotalBooks = await _context.Book.CountAsync();

            // Total revenue (sum of order prices)
            ViewBag.TotalRevenue = await _context.Order.SumAsync(o => o.order_price);

            return View();
        }

        public IActionResult GenerateReports()
        {
            return View();
        }

        public IActionResult GenerateBooksReport()
        {
            var books = _context.Book.ToList();
            return GenerateExcelFile(books, "BooksReport");
        }

        public IActionResult GenerateCustomersReport()
        {
            var customers = _context.Customer.ToList();
            return GenerateExcelFile(customers, "CustomersReport");
        }

        public IActionResult GenerateOrdersReport()
        {
            var orders = _context.Order.ToList();
            return GenerateExcelFile(orders, "OrdersReport");
        }

        public IActionResult GenerateBookFeedbackReport()
        {
            var feedbacks = _context.BookFeedback.ToList();
            return GenerateExcelFile(feedbacks, "BookFeedbackReport");
        }

        private IActionResult GenerateExcelFile<T>(IEnumerable<T> data, string fileName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).InsertTable(data); // Insert data as a table
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
                }
            }
        }
        public IActionResult ScanQRCode()
        {
            // Ensure you're returning the correct view
            return View();
        }


        //public IActionResult GetBookDetailsByQRCode(string qrCodeData)
        //{
        //    // Assuming the QR code contains the Book ID
        //    if (!int.TryParse(qrCodeData, out int bookId))
        //    {
        //        ViewBag.ErrorMessage = "Invalid QR code data. Could not extract Book ID.";
        //        return View("ScanQRCode");
        //    }

        //    // Debugging: Check the value of bookId
        //    Console.WriteLine($"Scanned Book ID: {bookId}");

        //    var book = _context.Book.FirstOrDefault(b => b.book_id == bookId);
        //    if (book == null)
        //    {
        //        ViewBag.ErrorMessage = "No book found for the scanned QR code.";
        //        return View("ScanQRCode");
        //    }

        //    // Debugging: Check if the book is found
        //    Console.WriteLine($"Found Book: {book.book_title}");

        //    return View("ScannedBookDetails", book);

        //}


        //[HttpPost]
        //public IActionResult SearchBooks(string searchQuery)
        //{
        //    if (string.IsNullOrEmpty(searchQuery))
        //    {
        //        return PartialView("_SearchResults", new List<Book>());
        //    }

        //    var book = _context.Book
        //                        .Where(b => b.book_title.Contains(searchQuery) || b.book_id.ToString() == searchQuery)
        //                        .Take(1000)
        //                        .ToList();

        //    return PartialView("_SearchResults", book);
        //}


        [HttpPost]
        public IActionResult SearchBooks(string searchQuery)
        {
            // If search query is empty, return an empty list
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("SearchResults", new List<Book>());
            }

            // Try to parse the search query as an integer (for book_id search)
            int bookId;
            bool isBookIdSearch = int.TryParse(searchQuery, out bookId);

            // Query the database for books matching the title or the book ID
            var books = _context.Book
                                .Where(b => b.book_title.Contains(searchQuery) ||
                                           (isBookIdSearch && b.book_id == bookId))
                                .Take(1000)
                                .ToList();
          
            // Return the partial view with the book results
            return PartialView("SearchResults", books);
        }
        public IActionResult GetBookImage(int bookId)
        {
            var book = _context.Book.FirstOrDefault(b => b.book_id == bookId);
            if (book != null && book.book_image != null)
            {
                // Return the image as a file stream with the appropriate content type
                return File(book.book_image, "image/jpeg"); // Adjust the MIME type if needed (e.g., image/png)
            }
            return NotFound(); // Return 404 if the image is not found
        }




        [HttpPost]
        public IActionResult ScanQRCode(string qrCodeData)
        {
            if (string.IsNullOrEmpty(qrCodeData))
            {
                // Handle invalid QR code
                ViewData["QRCodeImage"] = null;
                return View();
            }

            // Find books by title (assuming qrCodeData is the book title)
            var books = _context.Book.Where(b => b.book_title.Contains(qrCodeData)).ToList();

            if (books == null || books.Count == 0)
            {
                ViewData["QRCodeImage"] = null;
                ViewData["NoBooksFound"] = "No books found with that title.";
                return View();
            }

            // Pass the list of books to the view
            ViewData["QRCodeImage"] = null; // You can remove this if you don't want to show the QR image
            ViewData["Books"] = books;
            return View();
        }


    }
}
