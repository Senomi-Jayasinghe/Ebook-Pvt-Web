using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace E_Book_Pvt_Website.Models
{
    public class Book
    {
        [Key]
        public int book_id { get; set; }
        [Required]
        public string book_title { get; set; }
        public string book_description { get; set; }
        public string book_publisher { get; set; }
        public double book_price { get; set; }
        public int book_ISBN { get; set; }
        public int book_author_id { get; set; }
        public byte[] book_image { get; set; }
        public int book_quantity { get; set; }
        public string book_category { get; set; }
    }
}
