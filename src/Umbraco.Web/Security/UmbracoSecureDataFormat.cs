using System;
using Microsoft.Owin.Security;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// Custom secure format that ensures the Identity in the ticket is <see cref="UmbracoBackOfficeIdentity"/> and not just a ClaimsIdentity
    /// </summary>
    internal class UmbracoSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly int _loginTimeoutMinutes;
        private readonly ISecureDataFormat<AuthenticationTicket> _ticketDataFormat;

        public UmbracoSecureDataFormat(int loginTimeoutMinutes, ISecureDataFormat<AuthenticationTicket> ticketDataFormat)
        {
            _loginTimeoutMinutes = loginTimeoutMinutes;
            _ticketDataFormat = ticketDataFormat ?? throw new ArgumentNullException(nameof(ticketDataFormat));
        }

        public string Protect(AuthenticationTicket data)
        {
            var backofficeIdentity = (UmbracoBackOfficeIdentity)data.Identity;

            //create a new ticket based on the passed in tickets details, however, we'll adjust the expires utc based on the specified timeout mins
            var ticket = new AuthenticationTicket(backofficeIdentity,
                new AuthenticationProperties(data.Properties.Dictionary)
                {
                    IssuedUtc = data.Properties.IssuedUtc,
                    ExpiresUtc = data.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(_loginTimeoutMinutes),
                    AllowRefresh = data.Properties.AllowRefresh,
                    IsPersistent = data.Properties.IsPersistent,
                    RedirectUri = data.Properties.RedirectUri
                });

            return _ticketDataFormat.Protect(ticket);
        }

        /// <summary>
        /// Unprotects the cookie
        /// </summary>
        /// <param name="protectedText"></param>
        /// <returns></returns>
        public AuthenticationTicket Unprotect(string protectedText)
        {
            AuthenticationTicket decrypt;
            try
            {
                decrypt = _ticketDataFormat.Unprotect(protectedText);
                if (decrypt == null) return null;
            }
            catch (Exception)
            {
                return null;
            }

            UmbracoBackOfficeIdentity identity;

            try
            {
                identity = UmbracoBackOfficeIdentity.FromClaimsIdentity(decrypt.Identity);
            }
            catch (Exception)
            {
                //if it cannot be created return null, will be due to serialization errors in user data most likely due to corrupt cookies or cookies
                //for previous versions of Umbraco
                return null;
            }

            //return the ticket with a UmbracoBackOfficeIdentity
            var ticket = new AuthenticationTicket(identity, decrypt.Properties);

            return ticket;
        }
    }
}
