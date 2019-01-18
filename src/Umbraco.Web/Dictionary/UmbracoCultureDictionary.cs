using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Dictionary
{

    /// <summary>
    /// A culture dictionary that uses the Umbraco ILocalizationService
    /// </summary>
    /// <remarks>
    /// TODO: The ICultureDictionary needs to represent the 'fast' way to do dictionary item retrieval - for front-end and back office.
    /// The ILocalizationService is the service used for interacting with this data from the database which isn't all that fast
    /// (even though there is caching involved, if there's lots of dictionary items the caching is not great)
    /// </remarks>
    public class DefaultCultureDictionary : Core.Dictionary.ICultureDictionary
    {
        private readonly ILocalizationService _localizationService;
        private readonly IAppCache _requestCache;
        private readonly CultureInfo _specificCulture;

        public DefaultCultureDictionary()
            : this(Current.Services.LocalizationService, Current.AppCaches.RequestCache)
        { }

        public DefaultCultureDictionary(ILocalizationService localizationService, IAppCache requestCache)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        }

        public DefaultCultureDictionary(CultureInfo specificCulture)
            : this(Current.Services.LocalizationService, Current.AppCaches.RequestCache)
        {
            _specificCulture = specificCulture ?? throw new ArgumentNullException(nameof(specificCulture));
        }

        public DefaultCultureDictionary(CultureInfo specificCulture, ILocalizationService localizationService, IAppCache requestCache)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
            _specificCulture = specificCulture ?? throw new ArgumentNullException(nameof(specificCulture));
        }

        /// <summary>
        /// Returns the dictionary value based on the key supplied
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                var found = _localizationService.GetDictionaryItemByKey(key);
                if (found == null)
                {
                    return string.Empty;
                }

                var byLang = found.Translations.FirstOrDefault(x => x.Language.Equals(Language));
                if (byLang == null)
                {
                    return string.Empty;
                }

                return byLang.Value;
            }
        }

        /// <summary>
        /// Returns the current culture
        /// </summary>
        public CultureInfo Culture => _specificCulture ?? System.Threading.Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Returns the child dictionary entries for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>
        /// NOTE: The result of this is not cached anywhere - the underlying repository does not cache
        /// the child lookups because that is done by a query lookup. This method isn't used in our codebase
        /// so I don't think this is a performance issue but if devs are using this it could be optimized here.
        /// </remarks>
        public IDictionary<string, string> GetChildren(string key)
        {
            var result = new Dictionary<string, string>();

            var found = _localizationService.GetDictionaryItemByKey(key);
            if (found == null)
            {
                return result;
            }

            var children = _localizationService.GetDictionaryItemChildren(found.Key);
            if (children == null)
            {
                return result;
            }

            foreach (var dictionaryItem in children)
            {
                var byLang = dictionaryItem.Translations.FirstOrDefault((x => x.Language.Equals(Language)));
                if (byLang != null)
                {
                    result.Add(dictionaryItem.ItemKey, byLang.Value);
                }
            }

            return result;
        }

        private ILanguage Language
        {
            get
            {
                //ensure it's stored/retrieved from request cache
                //NOTE: This is no longer necessary since these are cached at the runtime level, but we can leave it here for now.
                return _requestCache.GetCacheItem<ILanguage>(typeof (DefaultCultureDictionary).Name + "Culture" + Culture.Name,
                    () => _localizationService.GetLanguageByIsoCode(Culture.Name));
            }
        }
    }
}
