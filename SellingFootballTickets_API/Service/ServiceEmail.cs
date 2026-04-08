using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace SellingFootballTickets_API.Service
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "Ticket Football";
        public string SenderEmail { get; set; } = "lysovanra99@gmail.com";
        public string Username { get; set; } = "lysovanra99@gmail.com";
        public string Password { get; set; } = "ysly vdmq lull vmwh";
        public bool UseSsl { get; set; } = true;
    }

    public class ServiceEmail : IOTPService
    {
        private readonly SmtpSettings _smtp;

        public ServiceEmail(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
        }

        public string GenerateOTP(int length = 6)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                // crypto-secure single digit
                chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
            }
            return new string(chars);
        }

        public async Task SendOTPAsync(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(otp)) throw new ArgumentNullException(nameof(otp));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Your OTP Code";
            message.Body = new TextPart("plain")
            {
                Text = $"Your OTP code is: {otp}. It will expire in 5 minutes."
            };

            using var client = new SmtpClient();
            var secure = _smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

            await client.ConnectAsync(_smtp.Host, _smtp.Port, secure);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}