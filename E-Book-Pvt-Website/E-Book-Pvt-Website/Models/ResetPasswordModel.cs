using System.ComponentModel.DataAnnotations;



namespace E_Book_Pvt_Website.Models
{
    public class ResetPasswordModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class VerifyCodeModel
    {
        [Required]
        public string VerificationCode { get; set; }
    }
}
