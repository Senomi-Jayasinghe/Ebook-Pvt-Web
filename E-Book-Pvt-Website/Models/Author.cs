using System.ComponentModel.DataAnnotations;

namespace E_Book_Pvt_Website.Models
{
    public class Author
    {
        [Key]
        public int author_id { get; set; }
        [Required]
        public string author_name { get; set; }
    }
}
