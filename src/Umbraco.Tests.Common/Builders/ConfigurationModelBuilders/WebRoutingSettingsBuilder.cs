using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Common.Builders
{
    public class WebRoutingSettingsBuilder : BuilderBase<WebRoutingSettings>
    {
        private bool? _trySkipIisCustomErrors;
        private bool? _internalRedirectPreservesTemplate;
        private bool? _disableAlternativeTemplates;
        private bool? _validateAlternativeTemplates;
        private bool? _disableFindContentByIdPath;
        private bool? _disableRedirectUrlTracking;
        private string _urlProviderMode;
        private string _umbracoApplicationUrl;

        public WebRoutingSettingsBuilder WithTrySkipIisCustomErrors(bool trySkipIisCustomErrors)
        {
            _trySkipIisCustomErrors = trySkipIisCustomErrors;
            return this;
        }

        public WebRoutingSettingsBuilder WithInternalRedirectPreservesTemplate(bool internalRedirectPreservesTemplate)
        {
            _internalRedirectPreservesTemplate = internalRedirectPreservesTemplate;
            return this;
        }

        public WebRoutingSettingsBuilder WithDisableAlternativeTemplates(bool disableAlternativeTemplates)
        {
            _disableAlternativeTemplates = disableAlternativeTemplates;
            return this;
        }

        public WebRoutingSettingsBuilder WithValidateAlternativeTemplates(bool validateAlternativeTemplates)
        {
            _validateAlternativeTemplates = validateAlternativeTemplates;
            return this;
        }

        public WebRoutingSettingsBuilder WithDisableFindContentByIdPath(bool disableFindContentByIdPath)
        {
            _disableFindContentByIdPath = disableFindContentByIdPath;
            return this;
        }

        public WebRoutingSettingsBuilder WithDisableRedirectUrlTracking(bool disableRedirectUrlTracking)
        {
            _disableRedirectUrlTracking = disableRedirectUrlTracking;
            return this;
        }

        public WebRoutingSettingsBuilder WithUrlProviderMode(string urlProviderMode)
        {
            _urlProviderMode = urlProviderMode;
            return this;
        }

        public WebRoutingSettingsBuilder WithUmbracoApplicationUrl(string umbracoApplicationUrl)
        {
            _umbracoApplicationUrl = umbracoApplicationUrl;
            return this;
        }

        public override WebRoutingSettings Build()
        {
            var trySkipIisCustomErrors = _trySkipIisCustomErrors ?? false;
            var internalRedirectPreservesTemplate = _internalRedirectPreservesTemplate ?? false;
            var disableAlternativeTemplates = _disableAlternativeTemplates ?? false;
            var validateAlternativeTemplates = _validateAlternativeTemplates ?? false;
            var disableFindContentByIdPath = _disableFindContentByIdPath ?? false;
            var disableRedirectUrlTracking = _disableRedirectUrlTracking ?? false;
            var urlProviderMode = _urlProviderMode ?? UrlMode.Auto.ToString();
            var umbracoApplicationUrl = _umbracoApplicationUrl ?? string.Empty;

            return new WebRoutingSettings
            {
                TrySkipIisCustomErrors = trySkipIisCustomErrors,
                InternalRedirectPreservesTemplate = internalRedirectPreservesTemplate,
                DisableAlternativeTemplates = disableAlternativeTemplates,
                ValidateAlternativeTemplates = validateAlternativeTemplates,
                DisableFindContentByIdPath = disableFindContentByIdPath,
                DisableRedirectUrlTracking = disableRedirectUrlTracking,
                UrlProviderMode = urlProviderMode,
                UmbracoApplicationUrl = umbracoApplicationUrl,
            };
        }
    }
}
