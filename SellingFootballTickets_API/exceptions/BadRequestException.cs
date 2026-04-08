namespace SellingFootballTickets_API.exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(message, 400)
        { }
    }
}
