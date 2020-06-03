using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Claims;
using Umbraco.Core.BackOffice;

namespace Umbraco.Web.BackOffice.Security
{

    /// <summary>
    /// Custom secure format that ensures the Identity in the ticket is <see cref="UmbracoBackOfficeIdentity"/> and not just a ClaimsIdentity
    /// </summary>
    // TODO: Unsure if we really need this, there's no real reason why we have a custom Identity instead of just a ClaimsIdentity
    internal class UmbracoSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly int _loginTimeoutMinutes;
        private readonly ISecureDataFormat<AuthenticationTicket> _ticketDataFormat;

        public UmbracoSecureDataFormat(int loginTimeoutMinutes, ISecureDataFormat<AuthenticationTicket> ticketDataFormat)
        {
            _loginTimeoutMinutes = loginTimeoutMinutes;
            _ticketDataFormat = ticketDataFormat ?? throw new ArgumentNullException(nameof(ticketDataFormat));
        }
        
        public string Protect(AuthenticationTicket data, string purpose)
        {
            //create a new ticket based on the passed in tickets details, however, we'll adjust the expires utc based on the specified timeout mins
            var ticket = new AuthenticationTicket(data.Principal,
                new AuthenticationProperties(data.Properties.Items)
                {
                    IssuedUtc = data.Properties.IssuedUtc,
                    ExpiresUtc = data.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(_loginTimeoutMinutes),
                    AllowRefresh = data.Properties.AllowRefresh,
                    IsPersistent = data.Properties.IsPersistent,
                    RedirectUri = data.Properties.RedirectUri
                }, data.AuthenticationScheme);

            return _ticketDataFormat.Protect(ticket);
        }

        public string Protect(AuthenticationTicket data) => Protect(data, string.Empty);

        
        public AuthenticationTicket Unprotect(string protectedText) => Unprotect(protectedText, string.Empty);

        /// <summary>
        /// Un-protects the cookie
        /// </summary>
        /// <param name="protectedText"></param>
        /// <returns></returns>
        public AuthenticationTicket Unprotect(string protectedText, string purpose)
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
                identity = UmbracoBackOfficeIdentity.FromClaimsIdentity((ClaimsIdentity)decrypt.Principal.Identity);
            }
            catch (Exception)
            {
                //if it cannot be created return null, will be due to serialization errors in user data most likely due to corrupt cookies or cookies
                //for previous versions of Umbraco
                return null;
            }

            //return the ticket with a UmbracoBackOfficeIdentity
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), decrypt.Properties, decrypt.AuthenticationScheme);

            return ticket;
        }
    }
}
