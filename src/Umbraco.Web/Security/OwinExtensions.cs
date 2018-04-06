using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;

namespace Umbraco.Web.Security
{
    internal static class OwinExtensions
    {
        /// <summary>
        /// Gets the <see cref="ISecureDataFormat{AuthenticationTicket}"/> for the Umbraco back office cookie
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        internal static ISecureDataFormat<AuthenticationTicket> GetUmbracoAuthTicketDataProtector(this IOwinContext owinContext)
        {
            var found = owinContext.Get<UmbracoAuthTicketDataProtector>();
            return found?.Protector;
        }

    }
}
