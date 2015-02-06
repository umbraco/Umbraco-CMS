using System;
using System.Web.Security;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
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
            //TODO: Where to get the user data?
            //var userDataString = JsonConvert.SerializeObject(userdata);

            var ticket = new FormsAuthenticationTicket(
                5,
                data.Identity.Name,
                data.Properties.IssuedUtc.HasValue ? data.Properties.IssuedUtc.Value.LocalDateTime : DateTime.Now,
                data.Properties.ExpiresUtc.HasValue ? data.Properties.ExpiresUtc.Value.LocalDateTime : DateTime.Now.AddMinutes(_loginTimeoutMinutes),
                data.Properties.IsPersistent,
                "", //User data here!! This will come from the identity
                "/"
                );

            return FormsAuthentication.Encrypt(ticket);
        }

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

            return new AuthenticationTicket(identity, new AuthenticationProperties
            {
                ExpiresUtc = decrypt.Expiration.ToUniversalTime(),
                IssuedUtc = decrypt.IssueDate.ToUniversalTime(),
                IsPersistent = decrypt.IsPersistent
            });
        }
    }
}