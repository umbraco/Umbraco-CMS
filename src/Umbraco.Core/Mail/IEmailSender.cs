using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail
{
    /// <summary>
    /// Simple abstraction to send an email message
    /// </summary>
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message);

        Task SendAsync(EmailMessage message, bool enableNotification);

        bool CanSendRequiredEmail();
    }
}
