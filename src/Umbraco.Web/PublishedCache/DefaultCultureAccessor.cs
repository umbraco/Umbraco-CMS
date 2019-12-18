using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the default implementation of <see cref="IDefaultCultureAccessor"/>.
    /// </summary>
    public class DefaultCultureAccessor : IDefaultCultureAccessor
    {
        private readonly ILocalizationService _localizationService;
        private readonly RuntimeLevel _runtimeLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCultureAccessor"/> class.
        /// </summary>
        public DefaultCultureAccessor(ILocalizationService localizationService, IRuntimeState runtimeState)
        {
            _localizationService = localizationService;
            _runtimeLevel = runtimeState.Level;
        }

        /// <inheritdoc />
        public string DefaultCulture => _runtimeLevel == RuntimeLevel.Run
            ? _localizationService.GetDefaultLanguageIsoCode() ?? "" // fast
            : "en-US"; // default for install and upgrade, when the service is n/a
    }
}
