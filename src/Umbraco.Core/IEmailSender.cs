using System.Net.Mail;
using System.Threading.Tasks;

namespace Umbraco.Core
{
    /// <summary>
    /// Simple abstraction to send an email message
    /// </summary>
    public interface IEmailSender
    {
        Task SendAsync(MailMessage message);
    }
}