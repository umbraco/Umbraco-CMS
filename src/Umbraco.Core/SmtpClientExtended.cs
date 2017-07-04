using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

namespace Umbraco.Core
{
    //
    // Summary:
    //     Extends SmtpClient to check for authentication settings stored in AppSettings
    internal class SmtpClientExtended : SmtpClient
    {
        public SmtpClientExtended()
        {
            var smtpUserName = WebConfigurationManager.AppSettings["smtpUserName"];
            var smtpPassword = WebConfigurationManager.AppSettings["smtpPassword"];

            if (string.IsNullOrEmpty(smtpUserName) == false && string.IsNullOrEmpty(smtpPassword) == false)
            {
                this.UseDefaultCredentials = false;

                var networkCredentials = new NetworkCredential(smtpUserName, smtpPassword);
                this.Credentials = networkCredentials;
            }
        }
    }
}
