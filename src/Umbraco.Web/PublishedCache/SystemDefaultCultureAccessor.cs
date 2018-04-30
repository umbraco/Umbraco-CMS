using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the default implementation of <see cref="ISystemDefaultCultureAccessor"/>.
    /// </summary>
    public class SystemDefaultCultureAccessor : ISystemDefaultCultureAccessor
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDefaultCultureAccessor"/> class.
        /// </summary>
        /// <param name="localizationService"></param>
        public SystemDefaultCultureAccessor(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public string DefaultCulture => _localizationService.GetDefaultLanguageIsoCode() ?? ""; // fast 
    }
}
