namespace SellingFootballTickets_API.exceptions
{
    public class UnauthorizationException : BaseException
    {
        protected UnauthorizationException(string message) : base(message, 403)
        {
        }
    }
}
