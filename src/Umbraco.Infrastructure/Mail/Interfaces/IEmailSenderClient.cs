using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces
{
    public interface IEmailSenderClient
    {
        public Task SendAsync(MimeMessage? message);
    }
}
