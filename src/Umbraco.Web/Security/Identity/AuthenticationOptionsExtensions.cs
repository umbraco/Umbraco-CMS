using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Security.Identity
{
    public static class AuthenticationOptionsExtensions
    {
        /// <summary>
        /// Configures the properties of the authentication description instance for use with Umbraco back office
        /// </summary>
        /// <param name="options"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <param name="callbackPath">
        /// This is important if the identity provider is to be able to authenticate when upgrading Umbraco. We will try to extract this from
        /// any options passed in via reflection since none of the default OWIN providers inherit from a base class but so far all of them have a consistent
        /// name for the 'CallbackPath' property which is of type PathString. So we'll try to extract it if it's not found or supplied.
        /// 
        /// If a value is extracted or supplied, this will be added to an internal list which the UmbracoModule will use to allow the request to pass
        /// through without redirecting to the installer.
        /// </param>
        public static void ForUmbracoBackOffice(this AuthenticationOptions options, string style, string icon, string callbackPath = null)
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

            if (callbackPath.IsNullOrWhiteSpace())
            {
                try
                {
                    //try to get it with reflection
                    var prop = options.GetType().GetProperty("CallbackPath");
                    if (prop != null && TypeHelper.IsTypeAssignableFrom<PathString>(prop.PropertyType))
                    {
                        //get the value
                        var path = (PathString) prop.GetValue(options);
                        if (path.HasValue)
                        {
                            UmbracoModule.ReservedPaths.TryAdd(path.ToString());
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    LogHelper.Error(typeof (AuthenticationOptionsExtensions), "Could not read AuthenticationOptions properties", ex);
                }
            }
            else
            {
                UmbracoModule.ReservedPaths.TryAdd(callbackPath);
            }
        }
    }
}