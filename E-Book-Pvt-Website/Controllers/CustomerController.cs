using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CustomerList()
        {
            var customers = await _context.Customer.ToListAsync();
            return View(customers);
        }
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(int id, Customer customer)
        {
            if (id != customer.customer_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing customer record
                    var existingCustomer = await _context.Customer.FirstOrDefaultAsync(c => c.customer_id == id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Update customer details
                    existingCustomer.customer_name = customer.customer_name;
                    existingCustomer.customer_phoneno = customer.customer_phoneno;
                    existingCustomer.customer_address = customer.customer_address;
                    existingCustomer.customer_email = customer.customer_email;
                    existingCustomer.customer_password = customer.customer_password;

                    // Save changes to the database
                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Customer.Any(c => c.customer_id == customer.customer_id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CustomerList)); // Redirect to the list of customers after editing
            }

            if (!ModelState.IsValid)
            {
                // Iterate over the ModelState errors and print them to the console
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {modelError.ErrorMessage}");
                }
            }

            return View(customer); // Return to the edit view if the model is invalid
        }
    }
}
