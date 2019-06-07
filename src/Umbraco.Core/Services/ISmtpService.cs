using System.Threading.Tasks;

namespace Umbraco.Core.Services
{
    public interface ISmtpService
    {
        Task SendAsync(string mailMessage);
    }
}
