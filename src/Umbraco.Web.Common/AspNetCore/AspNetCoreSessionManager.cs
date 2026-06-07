using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
///     Resolves the current session identifier and reads, writes and clears session values using the
///     ASP.NET Core <see cref="ISession" /> exposed on the current <see cref="HttpContext" />.
/// </summary>
internal sealed class AspNetCoreSessionManager : ISessionIdResolver, ISessionManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<SessionOptions> _sessionOptions;
    private readonly IOptionsMonitor<LoggingSettings> _loggingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AspNetCoreSessionManager" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current <see cref="HttpContext" />.</param>
    /// <param name="sessionOptions">The configured session options, used to determine the session cookie name.</param>
    /// <param name="loggingSettings">The logging settings, used to determine how the session id is resolved for log enrichment.</param>
    public AspNetCoreSessionManager(
        IHttpContextAccessor httpContextAccessor,
        IOptions<SessionOptions> sessionOptions,
        IOptionsMonitor<LoggingSettings> loggingSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionOptions = sessionOptions;
        _loggingSettings = loggingSettings;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     The resolved value depends on <see cref="LoggingSettings.SessionIdLogging" />: the actual session id
    ///     (default), a one-way hash of the session cookie, or nothing.
    /// </remarks>
    public string? SessionId =>
        _loggingSettings.CurrentValue.SessionIdLogging switch
        {
            SessionIdLoggingMode.None => null,
            SessionIdLoggingMode.CookieHash => ResolveSessionCookieHash(),
            _ => ResolveSessionId(),
        };

    /// <summary>
    ///     Resolves the actual ASP.NET Core session id, but only when an established session cookie is present.
    /// </summary>
    /// <remarks>
    ///     Reading Session.Id forces a synchronous, blocking load from the session store. When sessions are
    ///     backed by IDistributedCache (e.g. load-balanced setups), that is a network round-trip incurred on
    ///     every request that resolves the id for logging - even anonymous requests that never use session.
    ///     Only an established session sends back the session cookie, so its absence means there is nothing
    ///     meaningful to load (see #23082).
    /// </remarks>
    private string? ResolveSessionId()
    {
        if (IsSessionsAvailable is false)
        {
            return "0";
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || TryGetSessionCookieValue(httpContext, out _) is false)
        {
            return null;
        }

        return httpContext.Session.Id;
    }

    /// <summary>
    ///     Resolves a one-way hash of the session cookie value, which correlates requests to the same session
    ///     without loading the session from its store.
    /// </summary>
    private string? ResolveSessionCookieHash()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || TryGetSessionCookieValue(httpContext, out var cookieValue) is false)
        {
            return null;
        }

        // Never log the raw cookie value - it is effectively a bearer token for the session. A one-way hash
        // preserves per-session correlation without exposing the cookie and without loading the session.
        return cookieValue!.GenerateHash<SHA256>();
    }

    private bool TryGetSessionCookieValue(HttpContext httpContext, out string? value)
    {
        var sessionCookieName = _sessionOptions.Value.Cookie.Name;
        if (sessionCookieName is null)
        {
            value = null;
            return false;
        }

        return httpContext.Request.Cookies.TryGetValue(sessionCookieName, out value);
    }

    /// <summary>
    ///     Gets a value indicating whether session is available for the current request.
    /// </summary>
    /// <remarks>
    ///     Accessing <see cref="HttpContext.Session" /> throws an <see cref="InvalidOperationException" /> when the
    ///     session middleware has not been configured (i.e. <c>UseSession</c> was not called), so this is checked
    ///     before reading from or writing to the session.
    /// </remarks>
    private bool IsSessionsAvailable => _httpContextAccessor.HttpContext?.Features.Get<ISessionFeature>()?.Session is not null;

    /// <inheritdoc />
    public string? GetSessionValue(string key)
    {
        if (!IsSessionsAvailable)
        {
            return null;
        }

        return _httpContextAccessor.HttpContext?.Session.GetString(key);
    }

    /// <inheritdoc />
    public void SetSessionValue(string key, string value)
    {
        if (!IsSessionsAvailable)
        {
            return;
        }

        _httpContextAccessor.HttpContext?.Session.SetString(key, value);
    }

    /// <inheritdoc />
    public void ClearSessionValue(string key)
    {
        if (!IsSessionsAvailable)
        {
            return;
        }

        _httpContextAccessor.HttpContext?.Session.Remove(key);
    }
}
