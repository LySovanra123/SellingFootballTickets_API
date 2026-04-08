namespace SellingFootballTickets_API.DTO
{
    public class UpdateTicketRequest
    {
        public char Row { get; set; }
        public string MatchDescription { get; set; }
        public DateTime MatchTime { get; set; }
        public string Description { get; set; }
        public string Stadium { get; set; }
        public decimal Price { get; set; }
        public DateTime KickOff { get; set; }
        public DateTime DateExpriseSale { get; set; }
        public int Block { get; set; }
        public char RowNew { get; set; }
    }
}
