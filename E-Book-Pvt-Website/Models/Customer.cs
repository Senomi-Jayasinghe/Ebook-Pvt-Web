using System.ComponentModel.DataAnnotations;

namespace E_Book_Pvt_Website.Models
{
    public class Customer
    {
        [Key]
        public int customer_id { get; set; }
        [Required]
        public string customer_name { get; set; }
        public string customer_phoneno { get; set; }
        public string customer_address { get; set; }
        public string customer_email { get; set; }
        public string customer_password { get; set; }
    }
}
