using System;
using System.Web.Security;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// An ISecureDataFormat that uses the old FormsAuthenticationTicket but encrypts ticket without relying on FormsAuthentication.
    /// </summary>
    /// <remarks>
    /// FormsAuthentication.Decrypt has an upper limit on input length of 4k characters - the max size of a single cookie.
    /// Umbraco however uses ChunkingCookieManager which supports longer cookie content, which it splits across multiple cookies.
    /// FormsAuthentication encryption and decryption was replaced by MachineKey encryption. FormsAuthentication uses MachineKey anyway,
    /// and using this key for encryption is important for load balanced scenarios, so this will keep working for people who already
    /// had this configured.
    /// </remarks>
    internal class MachineKeyEncryptedFormsAuthSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly int _loginTimeoutMinutes;

        public MachineKeyEncryptedFormsAuthSecureDataFormat(int loginTimeoutMinutes)
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
                data.Properties.IssuedUtc?.LocalDateTime ?? DateTime.Now,
                data.Properties.ExpiresUtc?.LocalDateTime
                ?? DateTime.Now.AddMinutes(_loginTimeoutMinutes),
                data.Properties.IsPersistent,
                userDataString,
                "/"
            );

            return AuthenticationExtensions.EncryptFormsAuthTicketWithMachineKey(ticket);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            FormsAuthenticationTicket ticket;
            UmbracoBackOfficeIdentity identity;

            try
            {
                ticket = AuthenticationExtensions.DecryptFormsAuthTicketWithMachineKey(protectedText);
                if (ticket == null)
                    return null;
                identity = new UmbracoBackOfficeIdentity(ticket);
            }
            catch (Exception)
            {
                //if it cannot be created return null, will be due to serialization errors in user data most likely due to corrupt cookies or cookies
                //for previous versions of Umbraco
                return null;
            }

            var result = new AuthenticationTicket(identity, new AuthenticationProperties
            {
                ExpiresUtc = ticket.Expiration.ToUniversalTime(),
                IssuedUtc = ticket.IssueDate.ToUniversalTime(),
                IsPersistent = ticket.IsPersistent,
                AllowRefresh = true
            });

            return result;
        }
    }
}
