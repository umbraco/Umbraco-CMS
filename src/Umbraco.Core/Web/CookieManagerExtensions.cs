// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

public static class CookieManagerExtensions
{
    public static string? GetPreviewCookieValue(this ICookieManager cookieManager) =>
        cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
}
