using System.ComponentModel.DataAnnotations;

namespace SellingFootballTickets_API.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; } 
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string Status { get; set; }
        public Orders Order { get; set; }
    }
}
