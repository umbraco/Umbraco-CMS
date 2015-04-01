using Microsoft.Owin.Security;
using Umbraco.Core;

namespace Umbraco.Web.Security.Identity
{
    public static class AuthenticationDescriptionOptionsExtensions
    {
        /// <summary>
        /// Configures the properties of the authentication description instance for use with Umbraco back office
        /// </summary>
        /// <param name="options"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        public static void ForUmbracoBackOffice(this AuthenticationOptions options, string style, string icon)
        {
            Mandate.ParameterNotNullOrEmpty(options.AuthenticationType, "options.AuthenticationType");

            //Ensure the prefix is set
            if (options.AuthenticationType.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix) == false)
            {
                options.AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationTypePrefix + options.AuthenticationType;    
            }

            options.Description.Properties["SocialStyle"] = style;
            options.Description.Properties["SocialIcon"] = icon;

            //flag for use in back office
            options.Description.Properties["UmbracoBackOffice"] = true;
        }
    }
}