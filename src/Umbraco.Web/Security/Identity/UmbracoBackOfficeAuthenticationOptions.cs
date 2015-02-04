using Microsoft.Owin.Security;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Umbraco auth options - really just ensures that it is operating in Active mode
    /// </summary>
    public sealed class UmbracoBackOfficeAuthenticationOptions : AuthenticationOptions
    {
        public UmbracoBackOfficeAuthenticationOptions()
            : base("UmbracoBackOffice")
        {
            //Must be active, this needs to look at each request to determine  if it should execute, 
            // if set to passive this will not be the case
            AuthenticationMode = AuthenticationMode.Active;
        }
    }
}