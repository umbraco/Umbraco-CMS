using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
///     Custom Umbraco options configuration for <see cref="RequestLocalizationOptions" />
/// </summary>
public class UmbracoRequestLocalizationOptions : IConfigureOptions<RequestLocalizationOptions>
{
    private readonly GlobalSettings _globalSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoRequestLocalizationOptions" /> class.
    /// </summary>
    public UmbracoRequestLocalizationOptions(IOptions<GlobalSettings> globalSettings) =>
        _globalSettings = globalSettings.Value;

    /// <inheritdoc />
    public void Configure(RequestLocalizationOptions options)
    {
        // set the default culture to what is in config
        options.DefaultRequestCulture = new RequestCulture(_globalSettings.DefaultUILanguage);

        options.RequestCultureProviders.Insert(0, new UmbracoBackOfficeIdentityCultureProvider(options));
        options.RequestCultureProviders.Insert(1, new UmbracoPublishedContentCultureProvider(options));
    }
}
