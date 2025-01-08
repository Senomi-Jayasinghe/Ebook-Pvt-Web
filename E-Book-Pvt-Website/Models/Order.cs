using System.ComponentModel.DataAnnotations;

namespace E_Book_Pvt_Website.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }
        [Required]
        public int order_customer_id { get; set; }
        public double order_price { get; set; }
        public DateTime order_date { get; set; }
        public string order_address { get; set; }
        public string order_phoneno { get; set;}
        public string order_status { get; set;}
    }
}
