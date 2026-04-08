namespace SellingFootballTickets_API.exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, 404)
        {
        }
    }
}
