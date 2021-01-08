using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.Localization
{
    /// <summary>
    /// Custom Umbraco options configuration for <see cref="RequestLocalizationOptions"/>
    /// </summary>
    public class UmbracoRequestLocalizationOptions : IConfigureOptions<RequestLocalizationOptions>
    {
        private readonly IOptions<GlobalSettings> _globalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestLocalizationOptions"/> class.
        /// </summary>
        public UmbracoRequestLocalizationOptions(IOptions<GlobalSettings> globalSettings) => _globalSettings = globalSettings;

        /// <inheritdoc/>
        public void Configure(RequestLocalizationOptions options)
        {
            // set the default culture to what is in config
            options.DefaultRequestCulture = new RequestCulture(_globalSettings.Value.DefaultUILanguage);

            // add a custom provider
            if (options.RequestCultureProviders == null)
            {
                options.RequestCultureProviders = new List<IRequestCultureProvider>();
            }

            options.RequestCultureProviders.Insert(0, new UmbracoBackOfficeIdentityCultureProvider());
            options.RequestCultureProviders.Insert(1, new UmbracoPublishedContentCultureProvider());
        }
    }
}
