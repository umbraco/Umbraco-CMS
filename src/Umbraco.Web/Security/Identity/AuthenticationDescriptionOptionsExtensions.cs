using Microsoft.Owin.Security;

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
        public static void ForUmbracoBackOffice(this AuthenticationDescription options, string style, string icon)
        {
            options.Properties["SocialStyle"] = style;
            options.Properties["SocialIcon"] = icon;

            //flag for use in back office
            options.Properties["UmbracoBackOffice"] = true;
        }
    }
}