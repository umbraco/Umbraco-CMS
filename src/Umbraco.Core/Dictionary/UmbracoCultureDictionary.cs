using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Dictionary;

/// <summary>
///     A culture dictionary that uses the Umbraco ILocalizationService
/// </summary>
/// <remarks>
///     TODO: The ICultureDictionary needs to represent the 'fast' way to do dictionary item retrieval - for front-end and
///     back office.
///     The ILocalizationService is the service used for interacting with this data from the database which isn't all that
///     fast
///     (even though there is caching involved, if there's lots of dictionary items the caching is not great)
/// </remarks>
internal class DefaultCultureDictionary : ICultureDictionary
{
    private readonly ILocalizationService _localizationService;
    private readonly IAppCache _requestCache;
    private readonly CultureInfo? _specificCulture;

    /// <summary>
    ///     Default constructor which will use the current thread's culture
    /// </summary>
    /// <param name="localizationService"></param>
    /// <param name="requestCache"></param>
    public DefaultCultureDictionary(ILocalizationService localizationService, IAppCache requestCache)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
    }

    /// <summary>
    ///     Constructor for testing to specify a static culture
    /// </summary>
    /// <param name="specificCulture"></param>
    /// <param name="localizationService"></param>
    /// <param name="requestCache"></param>
    public DefaultCultureDictionary(CultureInfo specificCulture, ILocalizationService localizationService, IAppCache requestCache)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        _specificCulture = specificCulture ?? throw new ArgumentNullException(nameof(specificCulture));
    }

    /// <summary>
    ///     Returns the current culture
    /// </summary>
    public CultureInfo Culture => _specificCulture ?? Thread.CurrentThread.CurrentUICulture;

    private ILanguage? Language =>

        // ensure it's stored/retrieved from request cache
        // NOTE: This is no longer necessary since these are cached at the runtime level, but we can leave it here for now.
        _requestCache.GetCacheItem(
            typeof(DefaultCultureDictionary).Name + "Culture" + Culture.Name,
            () =>
            {
                // find a language that matches the current culture or any of its parent cultures
                CultureInfo culture = Culture;
                while (culture != CultureInfo.InvariantCulture)
                {
                    ILanguage? language = _localizationService.GetLanguageByIsoCode(culture.Name);
                    if (language != null)
                    {
                        return language;
                    }

                    culture = culture.Parent;
                }

                return null;
            });

    /// <summary>
    ///     Returns the dictionary value based on the key supplied
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? this[string key]
    {
        get
        {
            IDictionaryItem? found = _localizationService.GetDictionaryItemByKey(key);
            if (found == null)
            {
                return string.Empty;
            }

            IDictionaryTranslation? byLang =
                found.Translations.FirstOrDefault(x => x.Language?.Equals(Language) ?? false);
            if (byLang == null)
            {
                return string.Empty;
            }

            return byLang.Value;
        }
    }

    /// <summary>
    ///     Returns the child dictionary entries for a given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <remarks>
    ///     NOTE: The result of this is not cached anywhere - the underlying repository does not cache
    ///     the child lookups because that is done by a query lookup. This method isn't used in our codebase
    ///     so I don't think this is a performance issue but if devs are using this it could be optimized here.
    /// </remarks>
    public IDictionary<string, string> GetChildren(string key)
    {
        var result = new Dictionary<string, string>();

        IDictionaryItem? found = _localizationService.GetDictionaryItemByKey(key);
        if (found == null)
        {
            return result;
        }

        IEnumerable<IDictionaryItem>? children = _localizationService.GetDictionaryItemChildren(found.Key);
        if (children == null)
        {
            return result;
        }

        foreach (IDictionaryItem dictionaryItem in children)
        {
            IDictionaryTranslation? byLang = dictionaryItem.Translations.FirstOrDefault(x => x.Language?.Equals(Language) ?? false);
            if (byLang != null && dictionaryItem.ItemKey is not null && byLang.Value is not null)
            {
                result.Add(dictionaryItem.ItemKey, byLang.Value);
            }
        }

        return result;
    }
}
