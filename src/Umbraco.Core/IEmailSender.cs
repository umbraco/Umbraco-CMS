using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    /// <summary>
    /// Simple abstraction to send an email message
    /// </summary>
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message);
    }
}
