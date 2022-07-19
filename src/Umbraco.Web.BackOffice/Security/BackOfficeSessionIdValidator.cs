// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Used to validate a cookie against a user's session id
/// </summary>
/// <remarks>
///     <para>
///         This uses another cookie to track the last checked time which is done for a few reasons:
///         * We can't use the user's auth ticket to do this because we'd be re-issuing the auth ticket all of the time and
///         it would never expire
///         plus the auth ticket size is much larger than this small value
///         * This will execute quite often (every minute per user) and in some cases there might be several requests that
///         end up re-issuing the cookie so the cookie value should be small
///         * We want to avoid the user lookup if it's not required so that will only happen when the time diff is great
///         enough in the cookie
///     </para>
///     <para>
///         This is a scoped/request based object.
///     </para>
/// </remarks>
public class BackOfficeSessionIdValidator
{
    public const string CookieName = "UMB_UCONTEXT_C";
    private readonly GlobalSettings _globalSettings;
    private readonly ISystemClock _systemClock;
    private readonly IBackOfficeUserManager _userManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeSessionIdValidator" /> class.
    /// </summary>
    public BackOfficeSessionIdValidator(ISystemClock systemClock, IOptionsSnapshot<GlobalSettings> globalSettings, IBackOfficeUserManager userManager)
    {
        _systemClock = systemClock;
        _globalSettings = globalSettings.Value;
        _userManager = userManager;
    }

    public async Task ValidateSessionAsync(TimeSpan validateInterval, CookieValidatePrincipalContext context)
    {
        if (!context.Request.IsBackOfficeRequest())
        {
            return;
        }

        var valid = await ValidateSessionAsync(validateInterval, context.HttpContext, context.Options.CookieManager, _systemClock, context.Properties.IssuedUtc, context.Principal?.Identity as ClaimsIdentity);

        if (valid == false)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(Constants.Security.BackOfficeAuthenticationType);
        }
    }

    private async Task<bool> ValidateSessionAsync(
        TimeSpan validateInterval,
        HttpContext httpContext,
        ICookieManager cookieManager,
        ISystemClock systemClock,
        DateTimeOffset? authTicketIssueDate,
        ClaimsIdentity? currentIdentity)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (cookieManager == null)
        {
            throw new ArgumentNullException(nameof(cookieManager));
        }

        if (systemClock == null)
        {
            throw new ArgumentNullException(nameof(systemClock));
        }

        if (currentIdentity == null)
        {
            return false;
        }

        DateTimeOffset? issuedUtc = null;
        DateTimeOffset currentUtc = systemClock.UtcNow;

        // read the last checked time from a custom cookie
        var lastCheckedCookie = cookieManager.GetRequestCookie(httpContext, CookieName);

        if (lastCheckedCookie.IsNullOrWhiteSpace() == false)
        {
            if (DateTimeOffset.TryParse(lastCheckedCookie, out DateTimeOffset parsed))
            {
                issuedUtc = parsed;
            }
        }

        // no cookie, use the issue time of the auth ticket
        if (issuedUtc.HasValue == false)
        {
            issuedUtc = authTicketIssueDate;
        }

        // Only validate if enough time has elapsed
        var validate = issuedUtc.HasValue == false;
        if (issuedUtc.HasValue)
        {
            TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);
            validate = timeElapsed > validateInterval;
        }

        if (validate == false)
        {
            return true;
        }

        var userId = currentIdentity.GetUserId();
        BackOfficeIdentityUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var sessionId = currentIdentity.FindFirstValue(Constants.Security.SessionIdClaimType);
        if (await _userManager.ValidateSessionIdAsync(userId, sessionId) == false)
        {
            return false;
        }

        // we will re-issue the cookie last checked cookie
        cookieManager.AppendResponseCookie(
            httpContext,
            CookieName,
            DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = _globalSettings.UseHttps || httpContext.Request.IsHttps,
                Path = "/"
            });

        return true;
    }
}
