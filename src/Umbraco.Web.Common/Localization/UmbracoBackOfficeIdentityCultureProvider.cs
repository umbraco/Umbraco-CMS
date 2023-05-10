// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
/// Sets the request culture to the culture of the back office user, if one is determined to be in the request.
/// </summary>
public class UmbracoBackOfficeIdentityCultureProvider : DynamicRequestCultureProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoBackOfficeIdentityCultureProvider" /> class.
    /// </summary>
    /// <param name="localizationOptions">The localization options.</param>
    public UmbracoBackOfficeIdentityCultureProvider(RequestLocalizationOptions localizationOptions)
        : base(localizationOptions)
    { }

    /// <inheritdoc />
    protected sealed override ProviderCultureResult? GetProviderCultureResult(HttpContext httpContext)
        => httpContext.User.Identity?.GetCultureString() is string culture
        ? new ProviderCultureResult(culture)
        : null;
}
