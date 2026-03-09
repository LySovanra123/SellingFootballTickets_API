namespace SellingFootballTickets_API.Service
{
    public class BuyTicketRequest
    {
        public int UserId { get; set; }
        public string Row { get; set; }
        public int Quantity { get; set; }
    }
}
