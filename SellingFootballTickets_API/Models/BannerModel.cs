namespace SellingFootballTickets_API.Models
{
    public class BannerModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime StartDisplay { get; set; }
        public DateTime EndDisplay { get; set; }
    }
}
