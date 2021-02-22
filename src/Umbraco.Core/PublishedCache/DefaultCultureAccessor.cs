using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PublishedCache
{
    /// <summary>
    /// Provides the default implementation of <see cref="IDefaultCultureAccessor"/>.
    /// </summary>
    public class DefaultCultureAccessor : IDefaultCultureAccessor
    {
        private readonly ILocalizationService _localizationService;
        private readonly IOptions<GlobalSettings> _options;
        private readonly RuntimeLevel _runtimeLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCultureAccessor"/> class.
        /// </summary>
        public DefaultCultureAccessor(ILocalizationService localizationService, IRuntimeState runtimeState, IOptions<GlobalSettings> options)
        {
            _localizationService = localizationService;
            _options = options;
            _runtimeLevel = runtimeState.Level;
        }

        /// <inheritdoc />
        public string DefaultCulture => _runtimeLevel == RuntimeLevel.Run
            ? _localizationService.GetDefaultLanguageIsoCode() ?? "" // fast
            : _options.Value.DefaultUILanguage; // default for install and upgrade, when the service is n/a
    }
}
