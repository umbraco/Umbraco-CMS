using System;
using Microsoft.Owin.Security;
using Umbraco.Core;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// This is used so that we can retrive the auth ticket protector from an IOwinContext
    /// </summary>
    internal class UmbracoAuthTicketDataProtector : DisposableObjectSlim
    {
        public UmbracoAuthTicketDataProtector(ISecureDataFormat<AuthenticationTicket> protector)
        {
            Protector = protector ?? throw new ArgumentNullException(nameof(protector));
        }

        public ISecureDataFormat<AuthenticationTicket> Protector { get; }
    }
}
