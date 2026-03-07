using System.ComponentModel.DataAnnotations;

namespace SellingFootballTickets_API.Models
{
    public class Tickets
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string Code { get; set; }
        public string Description { get; set; }
        public string Stadium { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime KickOff { get; set; }
        public DateTime DateSale { get; set; }
        public DateTime DateExpriseSale { get; set; }
        public int Block  { get; set; }
        public char Row { get; set; }
        public int Seat { get; set; }
        public bool IsAvailable { get; set; }
    }
}
