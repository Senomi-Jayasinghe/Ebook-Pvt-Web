using System.ComponentModel.DataAnnotations;

namespace E_Book_Pvt_Website.Models
{
    public class CustomerLoginModal
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}


