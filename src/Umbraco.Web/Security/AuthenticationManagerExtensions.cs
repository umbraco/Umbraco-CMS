using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Constants = Umbraco.Core.Constants;

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
            var email = result.Identity.FindFirstValue(ClaimTypes.Email);
            return new ExternalLoginInfo
            {
                ExternalIdentity = result.Identity,
                Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value),
                DefaultUserName = name,
                Email = email
            };
        }

        public static IEnumerable<AuthenticationDescription> GetBackOfficeExternalLoginProviders(this IAuthenticationManager manager)
        {
            return manager.GetExternalAuthenticationTypes().Where(p => p.Properties.ContainsKey(Constants.Security.BackOfficeAuthenticationType));
        }

        /// <summary>
        /// Returns the authentication type for the last registered external login (oauth) provider that specifies an auto-login redirect option
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static string GetAutoLoginProvider(this IAuthenticationManager manager)
        {
            var found = manager.GetExternalAuthenticationTypes()
                .LastOrDefault(p => p.Properties.ContainsKey(Constants.Security.BackOfficeAuthenticationType)
                    && p.Properties.TryGetValue(Constants.Security.BackOfficeExternalLoginOptionsProperty, out var options)
                    && options is BackOfficeExternalLoginProviderOptions externalLoginProviderOptions
                    && externalLoginProviderOptions.AutoRedirectLoginToExternalProvider);

            return found?.AuthenticationType;
        }

        public static bool HasDenyLocalLogin(this IAuthenticationManager manager)
        {
            return manager.GetExternalAuthenticationTypes()
                .Any(p => p.Properties.ContainsKey(Constants.Security.BackOfficeAuthenticationType)
                    && p.Properties.TryGetValue(Constants.Security.BackOfficeExternalLoginOptionsProperty, out var options)
                    && options is BackOfficeExternalLoginProviderOptions externalLoginProviderOptions
                    && externalLoginProviderOptions.DenyLocalLogin);
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
    }
}
