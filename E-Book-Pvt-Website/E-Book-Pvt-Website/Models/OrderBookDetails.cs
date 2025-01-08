namespace E_Book_Pvt_Website.Models
{
    public class OrderBookDetails
    {
        public string BookTitle { get; set; }
        public int ISBN { get; set; }
        public byte[] Image { get; set; }
        public int AuthorId { get; set; }
        public int Quantity { get; set; }
    }
}
