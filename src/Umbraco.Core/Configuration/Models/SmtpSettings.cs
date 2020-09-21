using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    public class SmtpSettings : ValidatableEntryBase
    {
        [Required]
        [EmailAddress]
        public string From { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string PickupDirectoryLocation { get; set; }

        // See notes on ContentSettings.MacroErrors
        internal string DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network.ToString();

        public SmtpDeliveryMethod DeliveryMethodValue
        {
            get
            {
                return Enum.TryParse<SmtpDeliveryMethod>(DeliveryMethod, true, out var value)
                    ? value
                    : throw new InvalidOperationException(
                        $"Parsing of {nameof(DeliveryMethod)} field value of {DeliveryMethod} was not recognised as a valid value of the enum {nameof(SmtpDeliveryMethod)}. " +
                        $"This state shouldn't have been reached as if the configuration contains an invalid valie it should be caught by {nameof(GlobalSettingsValidator)}.");

            }
        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
