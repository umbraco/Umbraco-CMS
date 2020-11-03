using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Matches MailKit.Security.SecureSocketOptions and defined locally to avoid having to take
    /// thi
    /// </summary>
    public enum SecureSocketOptions
    {
        None = 0,
        Auto = 1,
        SslOnConnect = 2,
        StartTls = 3,
        StartTlsWhenAvailable = 4
    }

    public class SmtpSettings : ValidatableEntryBase
    {
        [Required]
        [EmailAddress]
        public string From { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.Auto;

        public string PickupDirectoryLocation { get; set; }

        public SmtpDeliveryMethod DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network;

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
