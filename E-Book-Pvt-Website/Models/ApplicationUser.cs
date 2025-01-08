using Microsoft.AspNetCore.Identity;

namespace E_Book_Pvt_Website.Models
{
    public class ApplicationUser : IdentityUser
    {

        // Add the VerificationCode property
        public string VerificationCode { get; set; }
    }
}
