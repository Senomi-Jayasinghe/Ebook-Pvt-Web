using E_Book_Pvt_Website.Data;
using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace E_Book_Pvt_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       


        [HttpGet]
        public IActionResult CustomerLogin()
        {
            return View(new CustomerLoginModal());
        }
        [HttpPost]
        public IActionResult CustomerLogin(CustomerLoginModal loginModel)
        {
            if (ModelState.IsValid)
            {
                // Check credentials
                bool isAuthenticated = AuthenticateCustomer(loginModel.Email, loginModel.Password);

                if (isAuthenticated)
                {
                    // Get customer_id based on the authenticated user
                    var customer = _context.Customer.FirstOrDefault(c => c.customer_email == loginModel.Email);

                    if (customer != null)
                    {
                        // Store customer_id and role_id in session
                        HttpContext.Session.SetInt32("customer_id", customer.customer_id);
                        HttpContext.Session.SetInt32("role_id", 1); // Set role_id as 1

                        TempData["Message"] = "Login successful!";
                        return RedirectToAction("BrowseBooks", "Book");
                    }
                }
                else
                {
                    // Invalid credentials
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Email is required.");
            }

            return View(loginModel);
        }

        private bool AuthenticateCustomer(string email, string password)
        {
            // Ensure email is not null or empty before querying
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Find the customer in the database
            var customer = _context.Customer
                .FirstOrDefault(c => c.customer_email == email);

            if (customer != null)
            {
                // Check if the entered password matches the stored password
                return customer.customer_password.Trim() == password.Trim();
            }

            return false; // Customer not found or password does not match
        }

        private bool VerifyPassword(string inputPassword, byte[] storedPassword)
        {
            // Example: Convert the input password to hash and compare
            // Replace with your hashing logic (e.g., using BCrypt, SHA256)
            var inputPasswordBytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
            return inputPasswordBytes.SequenceEqual(storedPassword);
        }

        // GET: SignUp page
        [HttpGet]
        public IActionResult SignUp()
        {
            return View(new CustomerSignUpViewModel());
        }

        // POST: Sign up new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(CustomerSignUpViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "The password and confirmation password do not match.");
            }
            else
            {
                var customer = new Customer
                {
                    customer_name = model.Name,
                    customer_email = model.Email,
                    customer_phoneno = model.PhoneNumber,
                    customer_address = model.Address,
                    customer_password = model.Password // Hash password if necessary
                };

                _context.Customer.Add(customer);
                _context.SaveChanges();
                TempData["Message"] = "Registration successful!";
                return RedirectToAction("CustomerLogin");
            }

            // If model state is invalid, return the same view with validation errors
            return View(model);
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View(new AdminLoginModal());
        }

        [HttpPost]
        public IActionResult AdminLogin(AdminLoginModal loginModel)
        {
            if (ModelState.IsValid)
            {
                // Check credentials
                bool isAuthenticated = AuthenticateAdmin(loginModel.Email, loginModel.Password);

                if (isAuthenticated)
                {
                    var admin = _context.Admin.FirstOrDefault(c => c.admin_email == loginModel.Email);

                    if (admin != null)
                    {
                        HttpContext.Session.SetInt32("admin_id", admin.admin_id);
                        HttpContext.Session.SetInt32("role_id", 2); // Set role_id as 2

                        TempData["Message"] = "Login successful!";
                        return RedirectToAction("AdminDashboard", "Admin");
                    }
                }
                else
                {
                    // Invalid credentials
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Email is required.");
            }

            return View(loginModel);
        }



        //    [HttpGet]
        //    public IActionResult ResetPassword()
        //    {
        //        return View();
        //    }


        //    [HttpPost]
        //    public async Task<IActionResult> ResetPassword(string email)
        //    {
        //        // Check if the email exists in the database
        //        var user = await _userManager.FindByEmailAsync(email);
        //        if (user == null)
        //        {
        //            // User not found
        //            ViewBag.ErrorMessage = "User not available.";
        //            return View();
        //        }

        //        // Generate a verification code
        //        var verificationCode = new Random().Next(100000, 999999).ToString();

        //        // Save the verification code to the database or cache (not shown here for simplicity)
        //        // Example: user.VerificationCode = verificationCode;
        //        // await _userManager.UpdateAsync(user);

        //        // Send the verification code via email
        //        try
        //        {
        //            var smtpClient = new SmtpClient("smtp.your-email-provider.com")
        //            {
        //                Port = 587,
        //                Credentials = new System.Net.NetworkCredential("somanpower68@gmail.com", "qzxg auts fcit fjew"),
        //                EnableSsl = true,
        //            };

        //            var mailMessage = new MailMessage
        //            {
        //                From = new MailAddress("somanpower68@gmail.com.com"),
        //                Subject = "Password Reset Verification Code",
        //                Body = $"Your verification code is: {verificationCode}",
        //                IsBodyHtml = true,
        //            };
        //            mailMessage.To.Add(email);

        //            await smtpClient.SendMailAsync(mailMessage);

        //            // Redirect to the verification page with the email as a parameter
        //            TempData["Email"] = email;
        //            return RedirectToAction("VerifyCode");
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.ErrorMessage = "Failed to send verification email. Please try again.";
        //            return View();
        //        }
        //    }

        //    [HttpGet]
        //    public IActionResult VerifyCode()
        //    {
        //        return View();
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> VerifyCode(string email, string verificationCode)
        //    {
        //        // Check the verification code (example logic)
        //        var user = await _userManager.FindByEmailAsync(email);
        //        if (user == null || user.VerificationCode != verificationCode)
        //        {
        //            ViewBag.ErrorMessage = "Invalid verification code.";
        //            return View();
        //        }

        //        // Redirect to reset password page
        //        TempData["Email"] = email;
        //        return RedirectToAction("SetNewPassword");
        //    }

        //    [HttpGet]
        //    public IActionResult SetNewPassword()
        //    {
        //        return View();
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> SetNewPassword(string email, string newPassword, string confirmPassword)
        //    {
        //        if (newPassword != confirmPassword)
        //        {
        //            ViewBag.ErrorMessage = "Passwords do not match.";
        //            return View();
        //        }

        //        var user = await _userManager.FindByEmailAsync(email);
        //        if (user == null)
        //        {
        //            ViewBag.ErrorMessage = "User not available.";
        //            return View();
        //        }

        //        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("CustomerLogin");
        //        }

        //        ViewBag.ErrorMessage = "Failed to reset password.";
        //        return View();
        //    }

        //public IActionResult ResetPasswordConfirmation()
        //    {
        //        return View();
        //    }

        private bool AuthenticateAdmin(string email, string password)
        {
            // Ensure email is not null or empty before querying
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var admin = _context.Admin
                .FirstOrDefault(c => c.admin_email == email);

            if (admin != null)
            {
                // Check if the entered password matches the stored password
                return admin.admin_password.Trim() == password.Trim();
            }

            return false;
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            // Check if the email exists in the Customer table
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.customer_email == email);

            // Check if the email exists in the Admin table
            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.admin_email == email);

            if (customer == null && admin == null)
            {
                // User not found in both tables
                ViewBag.ErrorMessage = "User not available. Please register.";
                return View();
            }

            // Generate a verification code
            var verificationCode = new Random().Next(100000, 999999).ToString();

            // Store the verification code in the session
            HttpContext.Session.SetString("VerificationCode", verificationCode);

            // Store the email in session
            HttpContext.Session.SetString("Email", email);

            // Send the verification code via email
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("somanpower68@gmail.com", "qzxg auts fcit fjew"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("somanpower68@gmail.com"),
                    Subject = "Password Reset Verification Code",
                    Body = $"Your verification code is: {verificationCode}",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);

                // Redirect to the verification page with the email as a parameter
                TempData["Email"] = email;
                return RedirectToAction("VerifyCode");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed to send verification email. Please try again.";
                return View();
            }

        }

        public IActionResult VerifyCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyCode(VerifyCodeModel model)
        {
            // Retrieve the stored code from the session
            var storedCode = HttpContext.Session.GetString("VerificationCode");

            if (storedCode == null || storedCode != model.VerificationCode)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return View(model);
            }

            // Store the verified email in session
            HttpContext.Session.SetString("VerifiedEmail", HttpContext.Session.GetString("Email"));

            return RedirectToAction("SetNewPassword");
        }

        //[HttpGet]
        //public IActionResult SetNewPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> SetNewPassword(string email, string newPassword, string confirmPassword)
        //{
        //    if (newPassword != confirmPassword)
        //    {
        //        ViewBag.ErrorMessage = "Passwords do not match.";
        //        return View();
        //    }

        //    // Check if the email exists in the Customer or Admin table
        //    var customer = await _context.Customer.FirstOrDefaultAsync(c => c.customer_email == email);
        //    var admin = await _context.Admin.FirstOrDefaultAsync(a => a.admin_email == email);

        //    if (customer == null && admin == null)
        //    {
        //        ViewBag.ErrorMessage = "User not available.";
        //        return View();
        //    }

        //    // Handle password reset for Customer
        //    if (customer != null)
        //    {
        //        // Update password for Customer (hash the password if needed)
        //        customer.customer_password = newPassword; // Replace with hashed password logic
        //        _context.Customer.Update(customer);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("CustomerLogin");
        //    }

        //    // Handle password reset for Admin
        //    if (admin != null)
        //    {
        //        // Update password for Admin (hash the password if needed)
        //        admin.admin_password = newPassword; // Replace with hashed password logic
        //        _context.Admin.Update(admin);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("AdminLogin");
        //    }

        //    ViewBag.ErrorMessage = "Failed to reset password.";
        //    return View();
        //}

        public IActionResult SetNewPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetNewPassword(SetNewPasswordModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View(model);
            }

            // Get the email from session
            var email = HttpContext.Session.GetString("VerifiedEmail");

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Session expired. Please try again.";
                return View(model);
            }

            // Find the user (either customer or admin)
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.customer_email == email);
            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.admin_email == email);

            if (customer == null && admin == null)
            {
                ViewBag.ErrorMessage = "User not found.";
                return View(model);
            }

            // Reset password for Customer
            if (customer != null)
            {
                customer.customer_password = model.NewPassword; // Replace with hashed password logic
                _context.Customer.Update(customer);
                await _context.SaveChangesAsync();
            }
            // Reset password for Admin
            else if (admin != null)
            {
                admin.admin_password = model.NewPassword; // Replace with hashed password logic
                _context.Admin.Update(admin);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Password reset successful. Please log in with your new password.";
            return RedirectToAction("CustomerLogin"); // Redirect to login page
        }

    }
}
