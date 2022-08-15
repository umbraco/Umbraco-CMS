using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Used to configure <see cref="CookieAuthenticationOptions" /> for the back office authentication type
/// </summary>
public class ConfigureBackOfficeCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly IBasicAuthService _basicAuthService;
    private readonly IDataProtectionProvider _dataProtection;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IIpResolver _ipResolver;
    private readonly IRuntimeState _runtimeState;
    private readonly SecuritySettings _securitySettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISystemClock _systemClock;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureBackOfficeCookieOptions" /> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" /></param>
    /// <param name="umbracoContextAccessor">The <see cref="IUmbracoContextAccessor" /></param>
    /// <param name="securitySettings">The <see cref="SecuritySettings" /> options</param>
    /// <param name="globalSettings">The <see cref="GlobalSettings" /> options</param>
    /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment" /></param>
    /// <param name="runtimeState">The <see cref="IRuntimeState" /></param>
    /// <param name="dataProtection">The <see cref="IDataProtectionProvider" /></param>
    /// <param name="userService">The <see cref="IUserService" /></param>
    /// <param name="ipResolver">The <see cref="IIpResolver" /></param>
    /// <param name="systemClock">The <see cref="ISystemClock" /></param>
    /// <param name="umbracoRequestPaths">The <see cref="UmbracoRequestPaths"/></param>
    /// <param name="basicAuthService">The <see cref="IBasicAuthService"/></param>
    public ConfigureBackOfficeCookieOptions(
        IServiceProvider serviceProvider,
        IUmbracoContextAccessor umbracoContextAccessor,
        IOptions<SecuritySettings> securitySettings,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        IDataProtectionProvider dataProtection,
        IUserService userService,
        IIpResolver ipResolver,
        ISystemClock systemClock,
        UmbracoRequestPaths umbracoRequestPaths,
        IBasicAuthService basicAuthService)
    {
        _serviceProvider = serviceProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
        _securitySettings = securitySettings.Value;
        _globalSettings = globalSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _runtimeState = runtimeState;
        _dataProtection = dataProtection;
        _userService = userService;
        _ipResolver = ipResolver;
        _systemClock = systemClock;
        _umbracoRequestPaths = umbracoRequestPaths;
        _basicAuthService = basicAuthService;
    }

    /// <inheritdoc />
    public void Configure(string name, CookieAuthenticationOptions options)
    {
        if (name != Constants.Security.BackOfficeAuthenticationType)
        {
            return;
        }

        Configure(options);
    }

    /// <inheritdoc />
    public void Configure(CookieAuthenticationOptions options)
    {
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = _globalSettings.TimeOut;
        options.Cookie.Domain = _securitySettings.AuthCookieDomain;
        options.Cookie.Name = _securitySettings.AuthCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            _globalSettings.UseHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";

        // For any redirections that may occur for the back office, they all go to the same path
        var backOfficePath = _globalSettings.GetBackOfficePath(_hostingEnvironment);
        options.AccessDeniedPath = backOfficePath;
        options.LoginPath = backOfficePath;
        options.LogoutPath = backOfficePath;

        options.DataProtectionProvider = _dataProtection;

        // NOTE: This is borrowed directly from aspnetcore source
        // Note: the purpose for the data protector must remain fixed for interop to work.
        IDataProtector dataProtector = options.DataProtectionProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            Constants.Security.BackOfficeAuthenticationType,
            "v2");
        var ticketDataFormat = new TicketDataFormat(dataProtector);

        options.TicketDataFormat = new BackOfficeSecureDataFormat(_globalSettings.TimeOut, ticketDataFormat);

        // Custom cookie manager so we can filter requests
        options.CookieManager = new BackOfficeCookieManager(
            _umbracoContextAccessor,
            _runtimeState,
            _umbracoRequestPaths,
            _basicAuthService);

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
                BackOfficeSecurityStampValidator securityStampValidator =
                    ctx.HttpContext.RequestServices.GetRequiredService<BackOfficeSecurityStampValidator>();

                // Same goes for the signinmanager
                IBackOfficeSignInManager signInManager =
                    ctx.HttpContext.RequestServices.GetRequiredService<IBackOfficeSignInManager>();

                ClaimsIdentity? backOfficeIdentity = ctx.Principal?.GetUmbracoIdentity();
                if (backOfficeIdentity == null)
                {
                    ctx.RejectPrincipal();
                    await signInManager.SignOutAsync();
                }

                // ensure the thread culture is set
                backOfficeIdentity?.EnsureCulture();

                EnsureTicketRenewalIfKeepUserLoggedIn(ctx);

                // add or update a claim to track when the cookie expires, we use this to track time remaining
                backOfficeIdentity?.AddOrUpdateClaim(new Claim(
                    Constants.Security.TicketExpiresClaimType,
                    ctx.Properties.ExpiresUtc!.Value.ToString("o"),
                    ClaimValueTypes.DateTime,
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeAuthenticationType,
                    backOfficeIdentity));

                await securityStampValidator.ValidateAsync(ctx);

                // This might have been called from GetRemainingTimeoutSeconds, in this case we don't want to ensure valid session
                // since that in it self will keep the session valid since we renew the lastVerified date.
                // Similarly don't renew the token
                if (IsRemainingSecondsRequest(ctx))
                {
                    return;
                }

                // This relies on IssuedUtc, so call it before updating it.
                await EnsureValidSessionId(ctx);

                // We have to manually specify Issued and Expires,
                // because the SecurityStampValidator refreshes the principal every 30 minutes,
                // When the principal is refreshed the Issued is update to time of refresh, however, the Expires remains unchanged
                // When we then try and renew, the difference of issued and expires effectively becomes the new ExpireTimeSpan
                // meaning we effectively lose 30 minutes of our ExpireTimeSpan for EVERY principal refresh if we don't
                // https://github.com/dotnet/aspnetcore/blob/main/src/Security/Authentication/Cookies/src/CookieAuthenticationHandler.cs#L115
                ctx.Properties.IssuedUtc = _systemClock.UtcNow;
                ctx.Properties.ExpiresUtc = _systemClock.UtcNow.Add(_globalSettings.TimeOut);
                ctx.ShouldRenew = true;
            },
            OnSigningIn = ctx =>
            {
                // occurs when sign in is successful but before the ticket is written to the outbound cookie
                ClaimsIdentity? backOfficeIdentity = ctx.Principal?.GetUmbracoIdentity();
                if (backOfficeIdentity != null)
                {
                    // generate a session id and assign it
                    // create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one
                    Guid session = _runtimeState.Level == RuntimeLevel.Run
                        ? _userService.CreateLoginSession(
                            backOfficeIdentity.GetId()!.Value,
                            _ipResolver.GetCurrentRequestIpAddress())
                        : Guid.NewGuid();

                    // add our session claim
                    backOfficeIdentity.AddClaim(new Claim(
                        Constants.Security.SessionIdClaimType,
                        session.ToString(),
                        ClaimValueTypes.String,
                        Constants.Security.BackOfficeAuthenticationType,
                        Constants.Security.BackOfficeAuthenticationType,
                        backOfficeIdentity));

                    // since it is a cookie-based authentication add that claim
                    backOfficeIdentity.AddClaim(new Claim(
                        ClaimTypes.CookiePath,
                        "/",
                        ClaimValueTypes.String,
                        Constants.Security.BackOfficeAuthenticationType,
                        Constants.Security.BackOfficeAuthenticationType,
                        backOfficeIdentity));
                }

                return Task.CompletedTask;
            },
            OnSignedIn = ctx =>
            {
                // occurs when sign in is successful and after the ticket is written to the outbound cookie

                // When we are signed in with the cookie, assign the principal to the current HttpContext
                ctx.HttpContext.SetPrincipalForRequest(ctx.Principal);

                return Task.CompletedTask;
            },
            OnSigningOut = ctx =>
            {
                // Clear the user's session on sign out
                if (ctx.HttpContext?.User?.Identity != null)
                {
                    var claimsIdentity = ctx.HttpContext.User.Identity as ClaimsIdentity;
                    var sessionId = claimsIdentity?.FindFirstValue(Constants.Security.SessionIdClaimType);
                    if (sessionId.IsNullOrWhiteSpace() == false && Guid.TryParse(sessionId, out Guid guidSession))
                    {
                        _userService.ClearLoginSession(guidSession);
                    }
                }

                // Remove all of our cookies
                var cookies = new[]
                {
                    BackOfficeSessionIdValidator.CookieName, _securitySettings.AuthCookieName,
                    Constants.Web.PreviewCookieName, Constants.Security.BackOfficeExternalCookieName,
                    Constants.Web.AngularCookieName, Constants.Web.CsrfValidationCookieName
                };
                foreach (var cookie in cookies)
                {
                    ctx.Options.CookieManager.DeleteCookie(ctx.HttpContext!, cookie, new CookieOptions { Path = "/" });
                }

                return Task.CompletedTask;
            }
        };
    }

    /// <summary>
    ///     Ensures that the user has a valid session id
    /// </summary>
    /// <remarks>
    ///     So that we are not overloading the database this throttles it's check to every minute
    /// </remarks>
    private async Task EnsureValidSessionId(CookieValidatePrincipalContext context)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        using IServiceScope scope = _serviceProvider.CreateScope();
        BackOfficeSessionIdValidator validator =
            scope.ServiceProvider.GetRequiredService<BackOfficeSessionIdValidator>();
        await validator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
    }

    /// <summary>
    ///     Ensures the ticket is renewed if the <see cref="SecuritySettings.KeepUserLoggedIn" /> is set to true
    ///     and the current request is for the get user seconds endpoint
    /// </summary>
    /// <param name="context">The <see cref="CookieValidatePrincipalContext" /></param>
    private void EnsureTicketRenewalIfKeepUserLoggedIn(CookieValidatePrincipalContext context)
    {
        if (!_securitySettings.KeepUserLoggedIn)
        {
            return;
        }

        DateTimeOffset currentUtc = _systemClock.UtcNow;
        DateTimeOffset? issuedUtc = context.Properties.IssuedUtc;
        DateTimeOffset? expiresUtc = context.Properties.ExpiresUtc;

        if (expiresUtc.HasValue && issuedUtc.HasValue)
        {
            TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);
            TimeSpan timeRemaining = expiresUtc.Value.Subtract(currentUtc);

            // if it's time to renew, then do it
            if (timeRemaining < timeElapsed)
            {
                context.ShouldRenew = true;
            }
        }
    }

    private bool IsRemainingSecondsRequest(CookieValidatePrincipalContext context)
    {
        RouteValueDictionary routeValues = context.HttpContext.Request.RouteValues;
        if (routeValues.TryGetValue("controller", out var controllerName) &&
            routeValues.TryGetValue("action", out var action))
        {
            if (controllerName?.ToString() == ControllerExtensions.GetControllerName<AuthenticationController>()
                && action?.ToString() == nameof(AuthenticationController.GetRemainingTimeoutSeconds))
            {
                return true;
            }
        }

        return false;
    }
}
