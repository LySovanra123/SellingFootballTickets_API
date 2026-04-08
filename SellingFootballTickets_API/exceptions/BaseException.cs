namespace SellingFootballTickets_API.exceptions
{
    public class BaseException : Exception
    {
        public int StatusCode { get; set; }
        protected BaseException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
