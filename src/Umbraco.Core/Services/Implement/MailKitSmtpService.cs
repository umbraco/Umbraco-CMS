using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Services.Implement
{
    internal class MailKitSmtpService : ISmtpService
    {
        private readonly ILogger _logger;
        public MailKitSmtpService(ILogger logger)
        {
            _logger = logger;
        }
        public async Task SendAsync(string mailMessage)
        {
            throw new NotImplementedException();
            await Task.FromResult(0);
        }
    }
}
