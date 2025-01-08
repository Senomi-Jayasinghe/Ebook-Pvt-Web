using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace E_Book_Pvt_Website.Controllers
{
    public class ResetPasswordController : Controller
    {


        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResetPasswordController> _logger;

        public ResetPasswordController(UserManager<IdentityUser> userManager, ILogger<ResetPasswordController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Request Password Reset
        [HttpGet]
        public IActionResult RequestPasswordReset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestPasswordReset(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email address not found.");
                    return View(model);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var verificationCode = token.Substring(0, 6); // Example for a short code

                // Send email logic here
                try
                {
                    var mailMessage = new MailMessage("no-reply@ebook.com", model.Email)
                    {
                        Subject = "Password Reset Code",
                        Body = $"Your verification code is: {verificationCode}",
                        IsBodyHtml = true
                    };

                    using var smtpClient = new SmtpClient("smtp.your-email-provider.com")
                    {
                        Credentials = new System.Net.NetworkCredential("your-email@example.com", "your-email-password"),
                        EnableSsl = true
                    };

                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email.");
                    ModelState.AddModelError("", "Failed to send verification code. Try again later.");
                    return View(model);
                }

                TempData["Email"] = model.Email;
                TempData["VerificationCode"] = verificationCode; // Store the code temporarily
                return RedirectToAction("VerifyCode");
            }

            return View(model);
        }

        // Verify Code
        [HttpGet]
        public IActionResult VerifyCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyCode(VerifyCodeModel model)
        {
            var storedCode = TempData["VerificationCode"] as string;
            if (storedCode == null || storedCode != model.VerificationCode)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return View(model);
            }

            TempData["VerifiedEmail"] = TempData["Email"];
            return RedirectToAction("ResetPassword");
        }

        // Reset Password
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var email = TempData["VerifiedEmail"] as string;
                if (email == null)
                {
                    ModelState.AddModelError("", "Session expired. Please restart the process.");
                    return RedirectToAction("RequestPasswordReset");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return RedirectToAction("RequestPasswordReset");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successfully.";
                    return RedirectToAction("CustomerLogin", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    
    public IActionResult Index()
        {
            return View();
        }
    }
}
