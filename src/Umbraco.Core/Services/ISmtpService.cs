using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface ISmtpService
    {
        Task SendAsync(MailMessage message);
    }
}
