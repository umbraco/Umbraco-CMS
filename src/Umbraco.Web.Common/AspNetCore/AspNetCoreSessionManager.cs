using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
///     Resolves the current session identifier and reads, writes and clears session values using the
///     ASP.NET Core <see cref="ISession" /> exposed on the current <see cref="HttpContext" />.
/// </summary>
internal sealed class AspNetCoreSessionManager : ISessionIdResolver, ISessionManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<SessionOptions> _sessionOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AspNetCoreSessionManager" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current <see cref="HttpContext" />.</param>
    /// <param name="sessionOptions">The configured session options, used to determine the session cookie name.</param>
    public AspNetCoreSessionManager(IHttpContextAccessor httpContextAccessor, IOptions<SessionOptions> sessionOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionOptions = sessionOptions;
    }

    /// <inheritdoc />
    public string? SessionId
    {
        get
        {
            if (IsSessionsAvailable is false)
            {
                return "0";
            }

            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            // Reading Session.Id forces a synchronous, blocking load from the session store. When sessions are
            // backed by IDistributedCache (e.g. load-balanced setups), that is a network round-trip incurred on
            // every request that resolves the id for logging - even anonymous requests that never use session.
            // Only an established session sends back the session cookie, so its absence means there is nothing
            // meaningful to load. (#23082)
            var sessionCookieName = _sessionOptions.Value.Cookie.Name;
            if (sessionCookieName is null || httpContext.Request.Cookies.ContainsKey(sessionCookieName) is false)
            {
                return null;
            }

            return httpContext.Session.Id;
        }
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
