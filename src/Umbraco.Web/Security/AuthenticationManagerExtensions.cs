using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using UserLoginInfo = Microsoft.AspNetCore.Identity.UserLoginInfo;

namespace Umbraco.Web.Security
{
    public static class AuthenticationManagerExtensions
    {
        private static ExternalLoginInfo GetExternalLoginInfo(AuthenticateResult result)
        {
            if (result == null || result.Identity == null)
            {
                return null;
            }
            var idClaim = result.Identity.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                return null;
            }
            // By default we don't allow spaces in user names
            var name = result.Identity.Name;
            if (name != null)
            {
                name = name.Replace(" ", "");
            }

            var email = result.Identity.FindFirst(ClaimTypes.Email)?.Value;
            return new ExternalLoginInfo
            {
                ExternalIdentity = result.Identity,
                Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value, idClaim.Issuer),
                DefaultUserName = name,
                Email = email
            };
        }

        /// <summary>
        ///     Extracts login info out of an external identity
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="authenticationType"></param>
        /// <param name="xsrfKey">key that will be used to find the userId to verify</param>
        /// <param name="expectedValue">
        ///     the value expected to be found using the xsrfKey in the AuthenticationResult.Properties
        ///     dictionary
        /// </param>
        /// <returns></returns>
        public static async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(this IAuthenticationManager manager,
            string authenticationType,
            string xsrfKey, string expectedValue)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            var result = await manager.AuthenticateAsync(authenticationType);
            // Verify that the userId is the same as what we expect if requested
            if (result != null &&
                result.Properties != null &&
                result.Properties.Dictionary != null &&
                result.Properties.Dictionary.ContainsKey(xsrfKey) &&
                result.Properties.Dictionary[xsrfKey] == expectedValue)
            {
                return GetExternalLoginInfo(result);
            }
            return null;
        }

        /// <summary>
        ///     Extracts login info out of an external identity
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public static async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(this IAuthenticationManager manager, string authenticationType)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            return GetExternalLoginInfo(await manager.AuthenticateAsync(authenticationType));
        }
        
        public static IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes(this IAuthenticationManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            return manager.GetAuthenticationTypes(d => d.Properties != null && d.Properties.ContainsKey("Caption"));
        }

        public static ClaimsIdentity CreateTwoFactorRememberBrowserIdentity(this IAuthenticationManager manager, string userId)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));

            var rememberBrowserIdentity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            rememberBrowserIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

            return rememberBrowserIdentity;
        }
    }

    public class ExternalLoginInfo
    {
        /// <summary>Associated login data</summary>
        public UserLoginInfo Login { get; set; }

        /// <summary>Suggested user name for a user</summary>
        public string DefaultUserName { get; set; }

        /// <summary>Email claim from the external identity</summary>
        public string Email { get; set; }

        /// <summary>The external identity</summary>
        public ClaimsIdentity ExternalIdentity { get; set; }
    }
}
