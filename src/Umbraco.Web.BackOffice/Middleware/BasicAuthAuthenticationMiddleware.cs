using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware
{
    /// <summary>
    ///     Ensures that preview pages (front-end routed) are authenticated with the back office identity appended to the
    ///     principal alongside any default authentication that takes place
    /// </summary>
    public class BasicAuthAuthenticationMiddleware : IMiddleware
    {
        private readonly ILogger<BasicAuthAuthenticationMiddleware> _logger;
        private readonly IOptionsSnapshot<BasicAuthSettings> _basicAuthSettings;
        private readonly IRuntimeState _runtimeState;

        public BasicAuthAuthenticationMiddleware(
            ILogger<BasicAuthAuthenticationMiddleware> logger,
            IOptionsSnapshot<BasicAuthSettings> basicAuthSettings,
            IRuntimeState runtimeState)
        {
            _logger = logger;
            _basicAuthSettings = basicAuthSettings;
            _runtimeState = runtimeState;
        }

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var options = _basicAuthSettings.Value;
            if (!options.Enabled || _runtimeState.Level < RuntimeLevel.Run)
            {
                await next(context);
                return;
            }

            var clientIPAddress = context.Connection.RemoteIpAddress;
            if (IsIpAllowListed(clientIPAddress, options.AllowedIPs))
            {
                await next(context);
                return;
            }

            AuthenticateResult authenticateResult = await context.AuthenticateBackOfficeAsync();
            if (authenticateResult.Succeeded)
            {
                await next(context);
                return;
            }


            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                //Extract credentials
                var encodedUsernamePassword = authHeader.Substring(6).Trim();
                var encoding = Encoding.UTF8;
                var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var seperatorIndex = usernamePassword.IndexOf(':');

                var username = usernamePassword.Substring(0, seperatorIndex);
                var password = usernamePassword.Substring(seperatorIndex + 1);


                IBackOfficeSignInManager backOfficeSignInManager =
                    context.RequestServices.GetRequiredService<IBackOfficeSignInManager>();


                SignInResult signInResult =
                    await backOfficeSignInManager.PasswordSignInAsync(username, password, false, true);

                if (signInResult.Succeeded)
                {
                    await next.Invoke(context);
                }
                else
                {
                    SetUnauthorizedHeader(context);
                }
            }
            else
            {
                // no authorization header
                SetUnauthorizedHeader(context);
            }
        }

        private bool IsIpAllowListed(IPAddress clientIpAddress, string[] allowlist)
        {
            foreach (var allowedIpString in allowlist)
            {
                if(IPAddress.TryParse(allowedIpString, out var allowedIp) && clientIpAddress.Equals(allowedIp))
                {
                    return true;
                };
            }

            return false;
        }

        private static void SetUnauthorizedHeader(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Umbraco as a Service login\"");
        }
    }
}
