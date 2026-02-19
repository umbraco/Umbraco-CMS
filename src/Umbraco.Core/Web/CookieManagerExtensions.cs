// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="ICookieManager"/>.
/// </summary>
public static class CookieManagerExtensions
{
    /// <summary>
    /// Gets the value of the preview cookie.
    /// </summary>
    /// <param name="cookieManager">The cookie manager.</param>
    /// <returns>The preview cookie value if found; otherwise, <c>null</c>.</returns>
    public static string? GetPreviewCookieValue(this ICookieManager cookieManager) =>
        cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
}
