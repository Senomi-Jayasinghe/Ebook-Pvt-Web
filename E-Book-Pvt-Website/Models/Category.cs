using System.ComponentModel.DataAnnotations;

namespace E_Book_Pvt_Website.Models
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        [Required]
        public string category_name { get; set; }

    }
}
