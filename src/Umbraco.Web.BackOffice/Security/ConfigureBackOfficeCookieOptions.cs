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
using Umbraco.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.Common.Security;

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
            BackOfficeSessionIdValidator sessionIdValidator)
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

            // TODO: Review these, we shouldn't really be redirecting at all, need to check the source to see if we can prevent any redirects.
            // I think we can do that by setting these to null in the events below, we cannot set them null here else they'll be replaced with defaults.
            // OK ... so figured it out, we need to have certain headers in the request to ensure that aspnetcore knows it's an ajax request,
            // see: https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Cookies/src/CookieAuthenticationEvents.cs#L43
            // and https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Cookies/src/CookieAuthenticationEvents.cs#L104
            // when those headers are set then it will respond with the correct status codes.
            // OR we override `CookieAuthenticationEvents` with our own and do
            // options.Events = new BackOfficeCookieAuthenticationEvents(); ... maybe that will give us more control anyways instead of using callbacks below?
            // Those methods like OnRedirectToLogin  are get/set so we can replace their logic, though actually looking at the code, if we replace these callbacks like
            // we are doing below then no redirections should occur but we may need to deal with the status code, we'll need to see
            options.AccessDeniedPath = _globalSettings.GetBackOfficePath(_hostingEnvironment);
            options.LoginPath = _globalSettings.GetBackOfficePath(_hostingEnvironment);
            options.LogoutPath = _globalSettings.GetBackOfficePath(_hostingEnvironment);

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
                    // TODO: We need to test this once we have signout functionality, not sure if the httpcontext.user.identity will still be set here
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
