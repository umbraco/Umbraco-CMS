using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Dictionary;

/// <summary>
///     A culture dictionary that uses the Umbraco ILanguageService and IDictionaryItemService.
/// </summary>
/// <remarks>
///     TODO: The ICultureDictionary needs to represent the 'fast' way to do dictionary item retrieval - for front-end and
///     back office.
///     ILanguageService and IDictionaryItemService are the services used for interacting with this data from the database
///     which isn't all that fast (even though there is caching involved, if there's lots of dictionary items the caching is
///     not great).
/// </remarks>
internal sealed class DefaultCultureDictionary : ICultureDictionary
{
    private readonly ILanguageService _languageService;
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IAppCache _requestCache;
    private readonly CultureInfo? _specificCulture;

    /// <summary>
    ///     Default constructor which will use the current thread's culture
    /// </summary>
    public DefaultCultureDictionary(ILanguageService languageService, IDictionaryItemService dictionaryItemService, IAppCache requestCache)
    {
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _dictionaryItemService = dictionaryItemService ?? throw new ArgumentNullException(nameof(dictionaryItemService));
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
    }

    /// <summary>
    ///     Constructor for testing to specify a static culture
    /// </summary>
    public DefaultCultureDictionary(CultureInfo specificCulture, ILanguageService languageService, IDictionaryItemService dictionaryItemService, IAppCache requestCache)
    {
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _dictionaryItemService = dictionaryItemService ?? throw new ArgumentNullException(nameof(dictionaryItemService));
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        _specificCulture = specificCulture ?? throw new ArgumentNullException(nameof(specificCulture));
    }

    /// <summary>
    ///     Returns the defualt umbraco's back office culture
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
                    ILanguage? language = _languageService.GetAsync(culture.Name).GetAwaiter().GetResult();
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
    public string this[string key]
    {
        get
        {
            IDictionaryItem? found = _dictionaryItemService.GetAsync(key).GetAwaiter().GetResult();
            if (found == null)
            {
                return string.Empty;
            }

            IDictionaryTranslation? byLang = found.Translations.FirstOrDefault(IsCurrentLanguage);
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

        IDictionaryItem? found = _dictionaryItemService.GetAsync(key).GetAwaiter().GetResult();
        if (found == null)
        {
            return result;
        }

        IEnumerable<IDictionaryItem>? children = _dictionaryItemService.GetChildrenAsync(found.Key).GetAwaiter().GetResult();
        if (children == null)
        {
            return result;
        }

        foreach (IDictionaryItem dictionaryItem in children)
        {
            IDictionaryTranslation? byLang = dictionaryItem.Translations.FirstOrDefault(IsCurrentLanguage);
            if (byLang != null && dictionaryItem.ItemKey is not null && byLang.Value is not null)
            {
                result.Add(dictionaryItem.ItemKey, byLang.Value);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified translation matches the current language.
    /// </summary>
    /// <param name="translation">The dictionary translation to check.</param>
    /// <returns><c>true</c> if the translation's language ISO code matches the current language; otherwise, <c>false</c>.</returns>
    private bool IsCurrentLanguage(IDictionaryTranslation translation) => translation.LanguageIsoCode.Equals(Language?.IsoCode);
}
