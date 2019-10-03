using System;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services.Implement
{
    internal class MailKitSmtpService : ISmtpService
    {
        private readonly ILogger _logger;
        public MailKitSmtpService(ILogger logger)
        {
            _logger = logger;
        }
        public async Task SendAsync(MailMessage mailMessage)
        {
            throw new NotImplementedException();
            await Task.FromResult(0);
        }
    }
}
