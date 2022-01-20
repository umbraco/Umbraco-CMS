using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Dictionary
{
    /// <summary>
    /// A culture dictionary factory used to create an Umbraco.Core.Dictionary.ICultureDictionary.
    /// </summary>
    /// <remarks>
    /// In the future this will allow use to potentially store dictionary items elsewhere and allows for maximum flexibility.
    /// </remarks>
    public class DefaultCultureDictionaryFactory : ICultureDictionaryFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly AppCaches _appCaches;

        public DefaultCultureDictionaryFactory(ILocalizationService localizationService, AppCaches appCaches)
        {
            _localizationService = localizationService;
            _appCaches = appCaches;
        }

        public ICultureDictionary CreateDictionary()
        {
            return new DefaultCultureDictionary(_localizationService, _appCaches.RequestCache);
        }
    }
}
