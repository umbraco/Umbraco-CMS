using System.Net.Mail;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using MailMessage = Umbraco.Core.Models.MailMessage;

namespace Umbraco.Core.Services.Implement
{
    public class NetMailSmtpService : ISmtpService
    {
        private readonly ILogger _logger;
        public NetMailSmtpService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(MailMessage message)
        {
            var netMailFrom = new System.Net.Mail.MailAddress(message.From.Address, message.From.DisplayName);
            var netMailTo = new System.Net.Mail.MailAddress(message.To.Address, message.To.DisplayName);

            var netMailMessage = new System.Net.Mail.MailMessage(netMailFrom, netMailTo)
                {Body = message.Body, IsBodyHtml = message.IsBodyHtml};

            await SendNetMailAsync(netMailMessage);
        }

        private async Task SendNetMailAsync(System.Net.Mail.MailMessage message)
        {
            using (var client = new SmtpClient())
            {
                await client.SendMailAsync(message);
            }
        }
    }
}
