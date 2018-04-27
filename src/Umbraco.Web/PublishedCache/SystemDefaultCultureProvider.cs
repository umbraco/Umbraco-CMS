using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the default implementation of <see cref="ISystemDefaultCultureProvider"/>.
    /// </summary>
    public class SystemDefaultCultureProvider : ISystemDefaultCultureProvider
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDefaultCultureProvider"/> class.
        /// </summary>
        /// <param name="localizationService"></param>
        public SystemDefaultCultureProvider(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public string DefaultCulture => _localizationService.GetDefaultLanguageIsoCode(); // capture - fast 
    }
}