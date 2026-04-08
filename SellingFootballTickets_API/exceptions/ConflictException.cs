namespace SellingFootballTickets_API.exceptions
{
    public class ConflictException : BaseException
    {
        protected ConflictException(string message) : base(message, 409)
        {
        }
    }
}
