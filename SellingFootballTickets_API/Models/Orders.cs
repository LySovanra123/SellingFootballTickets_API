using System.ComponentModel.DataAnnotations;

namespace SellingFootballTickets_API.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Users User { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotlePrice { get; set; }
        public string Status { get; set; }

        public ICollection<OrderTicket> OrderTickets { get; set; }
        public Payment Payment { get; set; }
    }
}
