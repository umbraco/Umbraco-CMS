using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;
using Umbraco.Net;
using Umbraco.Core.Security;
using Umbraco.Web;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to configure <see cref="CookieAuthenticationOptions"/> for the back office authentication type
    /// </summary>
    public class ConfigureBackOfficeCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ISecuritySettings _securitySettings;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly IDataProtectionProvider _dataProtection;
        private readonly IRequestCache _requestCache;
        private readonly IUserService _userService;
        private readonly IIpResolver _ipResolver;
        private readonly BackOfficeSessionIdValidator _sessionIdValidator;
        private readonly BackOfficeSecurityStampValidator _securityStampValidator;

        public ConfigureBackOfficeCookieOptions(
            IUmbracoContextAccessor umbracoContextAccessor,
            ISecuritySettings securitySettings,
            IGlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IDataProtectionProvider dataProtection,
            IRequestCache requestCache,
            IUserService userService,
            IIpResolver ipResolver,
            BackOfficeSessionIdValidator sessionIdValidator,
            BackOfficeSecurityStampValidator securityStampValidator)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _securitySettings = securitySettings;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _runtimeState = runtimeState;
            _dataProtection = dataProtection;
            _requestCache = requestCache;
            _userService = userService;
            _ipResolver = ipResolver;
            _sessionIdValidator = sessionIdValidator;
            _securityStampValidator = securityStampValidator;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != Constants.Security.BackOfficeAuthenticationType) return;
            Configure(options);
        }

        public void Configure(CookieAuthenticationOptions options)
        {
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes);
            options.Cookie.Domain = _securitySettings.AuthCookieDomain;
            options.Cookie.Name = _securitySettings.AuthCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = _globalSettings.UseHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
            options.Cookie.Path = "/";

            options.DataProtectionProvider = _dataProtection;

            // NOTE: This is borrowed directly from aspnetcore source
            // Note: the purpose for the data protector must remain fixed for interop to work.
            var dataProtector = options.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", Constants.Security.BackOfficeAuthenticationType, "v2");
            var ticketDataFormat = new TicketDataFormat(dataProtector);

            options.TicketDataFormat = new BackOfficeSecureDataFormat(_globalSettings.TimeOutInMinutes, ticketDataFormat);

            //Custom cookie manager so we can filter requests
            options.CookieManager = new BackOfficeCookieManager(
                _umbracoContextAccessor,
                _runtimeState,
                _hostingEnvironment,
                _globalSettings,
                _requestCache);
            // _explicitPaths); TODO: Implement this once we do OAuth somehow


            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = async ctx =>
                {
                    //ensure the thread culture is set
                    ctx.Principal?.Identity?.EnsureCulture();

                    await EnsureValidSessionId(ctx);

                    if (ctx.Principal?.Identity == null)
                    {
                        await ctx.HttpContext.SignOutAsync(Constants.Security.BackOfficeAuthenticationType);
                        return;
                    }

                    await _securityStampValidator.ValidateAsync(ctx);
                },
                OnSignedIn = ctx =>
                {
                    // When we are signed in with the cookie, assign the principal to the current HttpContext
                    ctx.HttpContext.User = ctx.Principal;

                    if (ctx.Principal.Identity is UmbracoBackOfficeIdentity backOfficeIdentity)
                    {
                        //generate a session id and assign it
                        //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one

                        var session = _runtimeState.Level == RuntimeLevel.Run
                            ? _userService.CreateLoginSession(backOfficeIdentity.Id, _ipResolver.GetCurrentRequestIpAddress())
                            : Guid.NewGuid();

                        backOfficeIdentity.SessionId = session.ToString();

                        //since it is a cookie-based authentication add that claim
                        backOfficeIdentity.AddClaim(new Claim(ClaimTypes.CookiePath, "/", ClaimValueTypes.String, UmbracoBackOfficeIdentity.Issuer, UmbracoBackOfficeIdentity.Issuer, backOfficeIdentity));
                    }

                    return Task.CompletedTask;
                },
                OnSigningOut = ctx =>
                {
                    //Clear the user's session on sign out
                    if (ctx.HttpContext?.User?.Identity != null)
                    {
                        var claimsIdentity = ctx.HttpContext.User.Identity as ClaimsIdentity;
                        var sessionId = claimsIdentity.FindFirstValue(Constants.Security.SessionIdClaimType);
                        if (sessionId.IsNullOrWhiteSpace() == false && Guid.TryParse(sessionId, out var guidSession))
                        {
                            _userService.ClearLoginSession(guidSession);
                        }
                    }

                    // Remove all of our cookies
                    var cookies = new[]
                    {
                        BackOfficeSessionIdValidator.CookieName,
                        _securitySettings.AuthCookieName,
                        Constants.Web.PreviewCookieName,
                        Constants.Security.BackOfficeExternalCookieName
                    };
                    foreach (var cookie in cookies)
                    {
                        ctx.Options.CookieManager.DeleteCookie(ctx.HttpContext, cookie, new CookieOptions
                        {
                            Path = "/"
                        });
                    }

                    return Task.CompletedTask;
                }
            };
        }

        /// <summary>
        /// Ensures that the user has a valid session id
        /// </summary>
        /// <remarks>
        /// So that we are not overloading the database this throttles it's check to every minute
        /// </remarks>
        private async Task EnsureValidSessionId(CookieValidatePrincipalContext context)
        {
            if (_runtimeState.Level == RuntimeLevel.Run)
                await _sessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
        }
    }
}
