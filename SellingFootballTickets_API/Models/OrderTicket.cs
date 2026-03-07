using System.ComponentModel.DataAnnotations;

namespace SellingFootballTickets_API.Models
{
    public class OrderTicket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int TicketId { get; set; }
        public Orders Order { get; set; }
        public Tickets Ticket { get; set; }
    }
}
