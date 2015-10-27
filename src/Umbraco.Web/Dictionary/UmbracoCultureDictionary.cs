using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Dictionary
{

    /// <summary>
    /// A culture dictionary that uses the Umbraco ILocalizationService
    /// </summary>
	public class DefaultCultureDictionary : Umbraco.Core.Dictionary.ICultureDictionary
	{
	    private readonly ILocalizationService _localizationService;
        private readonly ICacheProvider _requestCacheProvider;
        private readonly CultureInfo _specificCulture = null;

	    public DefaultCultureDictionary()
            : this(ApplicationContext.Current.Services.LocalizationService, ApplicationContext.Current.ApplicationCache.RequestCache)
	    {
	        
	    }

	    public DefaultCultureDictionary(ILocalizationService localizationService, ICacheProvider requestCacheProvider)
	    {
	        if (localizationService == null) throw new ArgumentNullException("localizationService");
	        if (requestCacheProvider == null) throw new ArgumentNullException("requestCacheProvider");
	        _localizationService = localizationService;
	        _requestCacheProvider = requestCacheProvider;
	    }

        public DefaultCultureDictionary(CultureInfo specificCulture)
            : this(ApplicationContext.Current.Services.LocalizationService, ApplicationContext.Current.ApplicationCache.RequestCache)
        {
            if (specificCulture == null) throw new ArgumentNullException("specificCulture");
            _specificCulture = specificCulture;
        }

        public DefaultCultureDictionary(CultureInfo specificCulture, ILocalizationService localizationService, ICacheProvider requestCacheProvider)
        {
            if (specificCulture == null) throw new ArgumentNullException("specificCulture");
            if (localizationService == null) throw new ArgumentNullException("localizationService");
            if (requestCacheProvider == null) throw new ArgumentNullException("requestCacheProvider");
            _localizationService = localizationService;
            _requestCacheProvider = requestCacheProvider;
            _specificCulture = specificCulture;
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
		public CultureInfo Culture
		{
		    get { return _specificCulture ?? System.Threading.Thread.CurrentThread.CurrentUICulture; }
		}

        /// <summary>
        /// Returns the child dictionary entries for a given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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
                return _requestCacheProvider.GetCacheItem<ILanguage>(typeof (DefaultCultureDictionary).Name + "Culture",
                    () => _localizationService.GetLanguageByIsoCode(Culture.Name));
            }
		}
	}

}
