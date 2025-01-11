using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Helpers;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Checkout(double totalPrice)
        {
            // Pass the total price to the checkout view
            var viewModel = new OrderViewModel
            {
                TotalPrice = totalPrice
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult PlaceOrder(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the customer ID from the session
                int? customerId = HttpContext.Session.GetInt32("customer_id");
                if (customerId == null) return RedirectToAction("CustomerLogin", "Account");

                // Insert into Order table
                var order = new Order
                {
                    order_customer_id = customerId.Value,
                    order_price = model.TotalPrice,
                    order_date = model.OrderDate,
                    order_address = model.OrderAddress,
                    order_phoneno = model.OrderPhoneNo,
                    order_status = "Placed"
                };

                _context.Order.Add(order);
                _context.SaveChanges();

                // Retrieve cart items from the session or database (assuming session here)
                var cartItems = GetCartItems();

                // Insert into OrderBook table
                foreach (var cartItem in cartItems)
                {
                    var orderBook = new OrderBook
                    {
                        order_id = order.order_id,      // Use the generated Order ID
                        book_id = cartItem.Book.book_id, // Each book in the cart
                        quantity = cartItem.Quantity,
                    };

                    _context.OrderBook.Add(orderBook);
                }

                _context.SaveChanges();

                // Clear the cart after placing the order
                ClearCart();

                return RedirectToAction("OrderConfirmation");
            }

            return View("Checkout", model); // Show errors if validation fails
        }

        private List<CartItem> GetCartItems()
        {
            // Retrieve cart items from session or database
            // Example for session-based cart
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void ClearCart()
        {
            // Clear cart items from session
            HttpContext.Session.Remove("Cart");
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        public IActionResult ViewOrders()
        {
            // Retrieve role_id and customer_id from session
            int? roleId = HttpContext.Session.GetInt32("role_id");
            int? customerId = HttpContext.Session.GetInt32("customer_id");

            // Get the orders from the database
            IEnumerable<Order> orders;

            if (roleId == 1 && customerId.HasValue)
            {
                // If role_id is 1, show only the orders for the logged-in customer
                orders = _context.Order.Where(o => o.order_customer_id == customerId.Value).ToList();
            }
            else
            {
                // If role_id is not 1, show all orders
                orders = _context.Order.ToList();
            }

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Order.FirstOrDefaultAsync(o => o.order_id == orderId);
            if (order == null) return NotFound();

            // Retrieve the author names as a dictionary
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);
            ViewBag.AuthorNames = authors;

            // Fetch order books along with book details
            var orderBooks = await _context.OrderBook
                .Where(ob => ob.order_id == orderId)
                .Select(ob => new OrderBookDetails
                {
                    Quantity = ob.quantity,
                    BookTitle = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_title).FirstOrDefault(),
                    ISBN = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_ISBN).FirstOrDefault(),
                    Image = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_image).FirstOrDefault(),
                    AuthorId = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_author_id).FirstOrDefault()
                })
                .ToListAsync();

            var viewModel = new OrderDetailsViewModel
            {
                Order = order,
                OrderBooks = orderBooks
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditOrder(int orderId)
        {
            var order = await _context.Order.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(); // Order not found in the database
            }

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> EditOrder(Order updatedOrder)
        {
            if (ModelState.IsValid)
            {
                // Find the order to update using the order_id
                var order = await _context.Order.FindAsync(updatedOrder.order_id);
                if (order == null)
                {
                    return NotFound();
                }

                // Update the order fields
                order.order_status = updatedOrder.order_status;
                order.order_address = updatedOrder.order_address;
                order.order_phoneno = updatedOrder.order_phoneno;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ViewOrders)); // Redirect to orders list after update
            }
            return View(updatedOrder);
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            // Retrieve the order by orderId
            var order = await _context.Order.FirstOrDefaultAsync(o => o.order_id == orderId);
            if (order == null)
            {
                return NotFound();  // Return a 404 if order not found
            }

            // Retrieve the author names as a dictionary for the view
            var authors = await _context.Author.ToDictionaryAsync(a => a.author_id, a => a.author_name);
            ViewBag.AuthorNames = authors;

            // Fetch order books along with book details
            var orderBooks = await _context.OrderBook
                .Where(ob => ob.order_id == orderId)
                .Select(ob => new OrderBookDetails
                {
                    Quantity = ob.quantity,
                    BookTitle = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_title).FirstOrDefault(),
                    ISBN = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_ISBN).FirstOrDefault(),
                    Image = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_image).FirstOrDefault(),
                    AuthorId = _context.Book.Where(b => b.book_id == ob.book_id).Select(b => b.book_author_id).FirstOrDefault()
                })
                .ToListAsync();

            var viewModel = new OrderDetailsViewModel
            {
                Order = order,
                OrderBooks = orderBooks
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("CancelOrder")]  // Specifies the name of the action when using POST
        public async Task<IActionResult> CancelOrderPost(int orderId)
        {
            // Retrieve the order by orderId
            var order = await _context.Order.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();  // Return a 404 if order not found
            }

            // Change the order status to "Cancelled"
            order.order_status = "Cancelled";

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the orders list or any other view
            return RedirectToAction(nameof(ViewOrders));  // Redirect to the list or view of orders
        }
    }
}
