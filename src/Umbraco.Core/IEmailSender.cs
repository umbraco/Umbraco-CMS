using System.Net.Mail;
using System.Threading.Tasks;

namespace Umbraco.Core
{
    /// <summary>
    /// Simple abstraction to send an email message
    /// </summary>
    public interface IEmailSender
    {
        // TODO: This would be better if MailMessage was our own abstraction!
        Task SendAsync(MailMessage message);
    }
}
