using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing content preview mode.
/// </summary>
/// <remarks>
///     Preview mode allows backoffice users to view unpublished content changes
///     as they would appear on the front-end website. This implementation uses
///     secure cookies with SameSite=None to support cross-site preview scenarios,
///     falling back to non-secure SameSite=Lax cookies for insecure (plain http) setups.
/// </remarks>
public class PreviewService : IPreviewService
{
    private readonly ICookieManager _cookieManager;
    private readonly GlobalSettings _globalSettings;
    private readonly IRequestAccessor _requestAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PreviewService" /> class.
    /// </summary>
    /// <param name="cookieManager">The cookie manager for handling preview cookies.</param>
    /// <param name="globalSettings">The global settings.</param>
    /// <param name="requestAccessor">The request accessor for determining the current request scheme.</param>
    public PreviewService(
        ICookieManager cookieManager,
        IOptions<GlobalSettings> globalSettings,
        IRequestAccessor requestAccessor)
    {
        _cookieManager = cookieManager;
        _globalSettings = globalSettings.Value;
        _requestAccessor = requestAccessor;
    }

    /// <inheritdoc />
    public Task<bool> TryEnterPreviewAsync(IUser user)
    {
        // Preview cookies use SameSite=None and Secure=true to support cross-site scenarios
        // (e.g., when the backoffice is on a different domain/port than the frontend during development).
        // SameSite=None requires Secure=true per browser specifications. However, browsers reject
        // Secure cookies set over plain http on anything but localhost, so for insecure setups
        // (e.g. http://<ip> development environments) fall back to a non-secure SameSite=Lax cookie.
        var secure = _globalSettings.UseHttps || _requestAccessor.GetRequestUrl()?.Scheme == Uri.UriSchemeHttps;
        _cookieManager.SetCookieValue(
            Constants.Web.PreviewCookieName,
            Constants.Web.PreviewCookieValue,
            httpOnly: true,
            secure: secure,
            sameSiteMode: secure ? "None" : "Lax");

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task EndPreviewAsync()
    {
        _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);
        return Task.CompletedTask;
    }
}
