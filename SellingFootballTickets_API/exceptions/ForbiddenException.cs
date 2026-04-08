namespace SellingFootballTickets_API.exceptions
{
    public class ForbiddenException : BaseException
    {
        protected ForbiddenException(string message) : base(message, 403)
        {
        }
    }
}
