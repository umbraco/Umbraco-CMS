using System.Net.Mail;

namespace Umbraco.Core.Configuration
{
    public interface ISmtpSettings
    {
        string From { get; }
        string Host { get; }
        int Port{ get; }
        string PickupDirectoryLocation { get; }
        SmtpDeliveryMethod DeliveryMethod { get; }
        string Username { get; }
        string Password { get; }
    }
}
