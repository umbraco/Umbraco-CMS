using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to configure <see cref="CookieAuthenticationOptions"/> for the back office authentication type
    /// </summary>
    public class ConfigureBackOfficeCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly SecuritySettings _securitySettings;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly IDataProtectionProvider _dataProtection;
        private readonly IRequestCache _requestCache;
        private readonly IUserService _userService;
        private readonly IIpResolver _ipResolver;
        private readonly ISystemClock _systemClock;
        private readonly LinkGenerator _linkGenerator;

        public ConfigureBackOfficeCookieOptions(
            IUmbracoContextAccessor umbracoContextAccessor,
            IOptions<SecuritySettings> securitySettings,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IDataProtectionProvider dataProtection,
            IRequestCache requestCache,
            IUserService userService,
            IIpResolver ipResolver,
            ISystemClock systemClock,
            LinkGenerator linkGenerator)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _securitySettings = securitySettings.Value;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _runtimeState = runtimeState;
            _dataProtection = dataProtection;
            _requestCache = requestCache;
            _userService = userService;
            _ipResolver = ipResolver;
            _systemClock = systemClock;
            _linkGenerator = linkGenerator;
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

            // For any redirections that may occur for the back office, they all go to the same path
            var backOfficePath = _globalSettings.GetBackOfficePath(_hostingEnvironment);
            options.AccessDeniedPath = backOfficePath;
            options.LoginPath = backOfficePath;
            options.LogoutPath = backOfficePath;

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
                _requestCache,
                _linkGenerator);
            // _explicitPaths); TODO: Implement this once we do OAuth somehow


            options.Events = new CookieAuthenticationEvents
            {
                // IMPORTANT! If you set any of OnRedirectToLogin, OnRedirectToAccessDenied, OnRedirectToLogout, OnRedirectToReturnUrl
                // you need to be aware that this will bypass the default behavior of returning the correct status codes for ajax requests and
                // not redirecting for non-ajax requests. This is because the default behavior is baked into this class here:
                // https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Cookies/src/CookieAuthenticationEvents.cs#L58
                // It would be possible to re-use the default behavior if any of these need to be set but that must be taken into account else
                // our back office requests will not function correctly. For now we don't need to set/configure any of these callbacks because
                // the defaults work fine with our setup.
                
                OnValidatePrincipal = async ctx =>
                {
                    // We need to resolve the BackOfficeSecurityStampValidator per request as a requirement (even in aspnetcore they do this)
                    var securityStampValidator = ctx.HttpContext.RequestServices.GetRequiredService<BackOfficeSecurityStampValidator>();
                    // Same goes for the signinmanager
                    var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<BackOfficeSignInManager>();

                    var backOfficeIdentity = ctx.Principal.GetUmbracoIdentity();
                    if (backOfficeIdentity == null)
                    {
                        ctx.RejectPrincipal();
                        await signInManager.SignOutAsync();
                    }

                    //ensure the thread culture is set
                    backOfficeIdentity.EnsureCulture();

                    await EnsureValidSessionId(ctx);
                    await securityStampValidator.ValidateAsync(ctx);
                    EnsureTicketRenewalIfKeepUserLoggedIn(ctx);

                    // add a claim to track when the cookie expires, we use this to track time remaining
                    backOfficeIdentity.AddClaim(new Claim(
                        Constants.Security.TicketExpiresClaimType,
                        ctx.Properties.ExpiresUtc.Value.ToString("o"),
                        ClaimValueTypes.DateTime,
                        UmbracoBackOfficeIdentity.Issuer,
                        UmbracoBackOfficeIdentity.Issuer,
                        backOfficeIdentity));

                },
                OnSigningIn = ctx =>
                {
                    // occurs when sign in is successful but before the ticket is written to the outbound cookie

                    var backOfficeIdentity = ctx.Principal.GetUmbracoIdentity();
                    if (backOfficeIdentity != null)
                    {
                        //generate a session id and assign it
                        //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one
                        var session = _runtimeState.Level == RuntimeLevel.Run
                            ? _userService.CreateLoginSession(backOfficeIdentity.Id, _ipResolver.GetCurrentRequestIpAddress())
                            : Guid.NewGuid();

                        //add our session claim
                        backOfficeIdentity.AddClaim(new Claim(Constants.Security.SessionIdClaimType, session.ToString(), ClaimValueTypes.String, UmbracoBackOfficeIdentity.Issuer, UmbracoBackOfficeIdentity.Issuer, backOfficeIdentity));
                        //since it is a cookie-based authentication add that claim
                        backOfficeIdentity.AddClaim(new Claim(ClaimTypes.CookiePath, "/", ClaimValueTypes.String, UmbracoBackOfficeIdentity.Issuer, UmbracoBackOfficeIdentity.Issuer, backOfficeIdentity));
                    }

                    return Task.CompletedTask;
                },
                OnSignedIn = ctx =>
                {
                    // occurs when sign in is successful and after the ticket is written to the outbound cookie

                    // When we are signed in with the cookie, assign the principal to the current HttpContext
                    ctx.HttpContext.User = ctx.Principal;                    

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
                        Constants.Security.BackOfficeExternalCookieName,
                        Constants.Web.AngularCookieName,
                        Constants.Web.CsrfValidationCookieName,
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
            {
                // TODO: MSDI this class is Singleton, BackOfficeSessionIdValidator is scoped
                // Are we OK to service locate in this instance?
                var validator = context.HttpContext.RequestServices.GetRequiredService<BackOfficeSessionIdValidator>();
                await validator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
            }
        }

        /// <summary>
        /// Ensures the ticket is renewed if the <see cref="SecuritySettings.KeepUserLoggedIn"/> is set to true
        /// and the current request is for the get user seconds endpoint
        /// </summary>
        /// <param name="context"></param>
        private void EnsureTicketRenewalIfKeepUserLoggedIn(CookieValidatePrincipalContext context)
        {
            if (!_securitySettings.KeepUserLoggedIn) return;

            var currentUtc = _systemClock.UtcNow;
            var issuedUtc = context.Properties.IssuedUtc;
            var expiresUtc = context.Properties.ExpiresUtc;

            if (expiresUtc.HasValue && issuedUtc.HasValue)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                //if it's time to renew, then do it
                if (timeRemaining < timeElapsed)
                {
                    context.ShouldRenew = true;
                }
            }
        }
    }
}
