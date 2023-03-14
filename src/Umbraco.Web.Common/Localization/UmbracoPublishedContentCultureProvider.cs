using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
///     Sets the request culture to the culture of the <see cref="IPublishedRequest" /> if one is found in the request
/// </summary>
public class UmbracoPublishedContentCultureProvider : RequestCultureProvider
{
    private readonly RequestLocalizationOptions _localizationOptions;
    private readonly object _locker = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoPublishedContentCultureProvider" /> class.
    /// </summary>
    public UmbracoPublishedContentCultureProvider(RequestLocalizationOptions localizationOptions) =>
        _localizationOptions = localizationOptions;

    /// <inheritdoc />
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        UmbracoRouteValues? routeValues = httpContext.Features.Get<UmbracoRouteValues>();
        if (routeValues != null)
        {
            var culture = routeValues.PublishedRequest.Culture;
            if (culture != null)
            {
                lock (_locker)
                {
                    // We need to dynamically change the supported cultures since we won't ever know what languages are used since
                    // they are dynamic within Umbraco. We have to handle this for both UI and Region cultures, in case people run different region and UI languages
                    // This code to check existence is borrowed from aspnetcore to avoid creating a CultureInfo
                    // https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Middleware/Localization/src/RequestLocalizationMiddleware.cs#L165
                    CultureInfo? existingCulture = _localizationOptions.SupportedCultures?.FirstOrDefault(
                        supportedCulture =>
                            StringSegment.Equals(supportedCulture.Name, culture, StringComparison.OrdinalIgnoreCase));

                    if (existingCulture == null)
                    {
                        // add this as a supporting culture
                        var ci = CultureInfo.GetCultureInfo(culture);
                        _localizationOptions.SupportedCultures?.Add(ci);
                    }

                    CultureInfo? existingUICulture = _localizationOptions.SupportedUICultures?.FirstOrDefault(
                        supportedCulture =>
                            StringSegment.Equals(supportedCulture.Name, culture, StringComparison.OrdinalIgnoreCase));

                    if (existingUICulture == null)
                    {
                        // add this as a supporting culture
                        var ci = CultureInfo.GetCultureInfo(culture);
                        _localizationOptions.SupportedUICultures?.Add(ci);
                    }
                }

                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
            }
        }

        return NullProviderCultureResult;
    }
}
