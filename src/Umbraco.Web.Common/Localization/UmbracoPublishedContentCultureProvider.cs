using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
/// Sets the request culture to the culture of the <see cref="IPublishedRequest" />, if one is found in the request.
/// </summary>
public class UmbracoPublishedContentCultureProvider : DynamicRequestCultureProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoPublishedContentCultureProvider" /> class.
    /// </summary>
    /// <param name="localizationOptions">The localization options.</param>
    public UmbracoPublishedContentCultureProvider(RequestLocalizationOptions localizationOptions)
        : base(localizationOptions)
    { }

    /// <inheritdoc />
    protected sealed override ProviderCultureResult? GetProviderCultureResult(HttpContext httpContext)
        => httpContext.Features.Get<UmbracoRouteValues>()?.PublishedRequest.Culture is string culture
        ? new ProviderCultureResult(culture)
        : null;
}
