using System;
using System.Security.Claims;
using System.Web.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Newtonsoft.Json;
using Owin;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security.Identity
{

    /// <summary>
    /// Custom secure format that uses the old FormsAuthentication format
    /// </summary>
    internal class FormsAuthenticationSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly int _loginTimeoutMinutes;

        public FormsAuthenticationSecureDataFormat(int loginTimeoutMinutes)
        {
            _loginTimeoutMinutes = loginTimeoutMinutes;
        }

        public string Protect(AuthenticationTicket data)
        {
            var backofficeIdentity = (UmbracoBackOfficeIdentity)data.Identity;
            var userDataString = JsonConvert.SerializeObject(backofficeIdentity.UserData);

            var ticket = new FormsAuthenticationTicket(
                5,
                data.Identity.Name,
                data.Properties.IssuedUtc.HasValue ? data.Properties.IssuedUtc.Value.LocalDateTime : DateTime.Now,
                data.Properties.ExpiresUtc.HasValue ? data.Properties.ExpiresUtc.Value.LocalDateTime : DateTime.Now.AddMinutes(_loginTimeoutMinutes),
                data.Properties.IsPersistent,
                userDataString,
                "/"
                );

            return FormsAuthentication.Encrypt(ticket);
        }

        /// <summary>
        /// Unprotects the cookie
        /// </summary>
        /// <param name="protectedText"></param>
        /// <returns></returns>
        public AuthenticationTicket Unprotect(string protectedText)
        {
            FormsAuthenticationTicket decrypt;
            try
            {
                decrypt = FormsAuthentication.Decrypt(protectedText);
                if (decrypt == null) return null;
            }
            catch (Exception)
            {
                return null;
            }

            var identity = new UmbracoBackOfficeIdentity(decrypt);

            var ticket = new AuthenticationTicket(identity, new AuthenticationProperties
            {
                ExpiresUtc = decrypt.Expiration.ToUniversalTime(),
                IssuedUtc = decrypt.IssueDate.ToUniversalTime(),
                IsPersistent = decrypt.IsPersistent
            });

            return ticket;
        }
    }
}