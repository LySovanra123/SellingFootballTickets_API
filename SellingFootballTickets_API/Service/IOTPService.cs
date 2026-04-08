namespace SellingFootballTickets_API.Service
{
    public interface IOTPService
    {
        string GenerateOTP(int length = 6);
        Task SendOTPAsync(string email, string otp);
    }
}
