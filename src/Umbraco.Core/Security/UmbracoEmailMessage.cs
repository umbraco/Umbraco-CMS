using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A custom implementation for IdentityMessage that allows the customization of how an email is sent
    /// </summary>
    internal class UmbracoEmailMessage : IdentityMessage
    {
        public IEmailSender MailSender { get; private set; }

        public UmbracoEmailMessage(IEmailSender mailSender)
        {
            MailSender = mailSender;
        }
    }
}
