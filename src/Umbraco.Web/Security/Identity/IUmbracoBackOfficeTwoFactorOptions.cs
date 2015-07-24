using Microsoft.Owin;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Used to display a custom view in the back office if developers choose to implement their own custom 2 factor authentication
    /// </summary>
    public interface IUmbracoBackOfficeTwoFactorOptions
    {
        string GetTwoFactorView(IOwinContext owinContext, UmbracoContext umbracoContext, string username);
    }
}