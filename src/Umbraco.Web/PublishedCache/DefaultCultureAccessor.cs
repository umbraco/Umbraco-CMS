using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides the default implementation of <see cref="IDefaultCultureAccessor"/>.
    /// </summary>
    public class DefaultCultureAccessor : IDefaultCultureAccessor
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCultureAccessor"/> class.
        /// </summary>
        /// <param name="localizationService"></param>
        public DefaultCultureAccessor(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public string DefaultCulture => _localizationService.GetDefaultLanguageIsoCode() ?? ""; // fast 
    }
}
