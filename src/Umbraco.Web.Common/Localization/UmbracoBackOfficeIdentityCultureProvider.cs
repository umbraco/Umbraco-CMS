// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
///     Sets the request culture to the culture of the back office user if one is determined to be in the request
/// </summary>
public class UmbracoBackOfficeIdentityCultureProvider : RequestCultureProvider
{
    private readonly RequestLocalizationOptions _localizationOptions;
    private readonly object _locker = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoBackOfficeIdentityCultureProvider" /> class.
    /// </summary>
    public UmbracoBackOfficeIdentityCultureProvider(RequestLocalizationOptions localizationOptions) =>
        _localizationOptions = localizationOptions;

    /// <inheritdoc />
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        CultureInfo? culture = httpContext.User.Identity?.GetCulture();

        if (culture is null)
        {
            return NullProviderCultureResult;
        }

        lock (_locker)
        {
            // We need to dynamically change the supported cultures since we won't ever know what languages are used since
            // they are dynamic within Umbraco. We have to handle this for both UI and Region cultures, in case people run different region and UI languages
            var cultureExists = _localizationOptions.SupportedCultures?.Contains(culture) ?? false;

            if (!cultureExists)
            {
                // add this as a supporting culture
                _localizationOptions.SupportedCultures?.Add(culture);
            }

            var uiCultureExists = _localizationOptions.SupportedCultures?.Contains(culture) ?? false;

            if (!uiCultureExists)
            {
                // add this as a supporting culture
                _localizationOptions.SupportedUICultures?.Add(culture);
            }

            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture.Name));
        }
    }
}
