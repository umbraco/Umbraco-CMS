// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Specifies the SameSite mode used when issuing a cookie.
/// </summary>
/// <remarks>
///     Mirrors <c>Microsoft.AspNetCore.Http.SameSiteMode</c>, numeric values included, so the two can be
///     cast between. The values are declared explicitly for that reason - do not renumber them. Core
///     cannot reference ASP.NET Core, which is why the enum is redeclared here rather than reused.
/// </remarks>
public enum CookieSameSiteMode
{
    /// <summary>
    ///     No SameSite attribute is written, leaving the browser to apply its own default.
    /// </summary>
    Unspecified = -1,

    /// <summary>
    ///     The cookie is sent on same-site and cross-site requests alike. Requires a secure (HTTPS) cookie.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The cookie is sent on same-site requests and on top-level cross-site navigations.
    /// </summary>
    Lax = 1,

    /// <summary>
    ///     The cookie is sent on same-site requests only.
    /// </summary>
    Strict = 2,
}
