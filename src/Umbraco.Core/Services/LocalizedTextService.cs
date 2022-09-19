using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class LocalizedTextService : ILocalizedTextService
{
    private readonly Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>>
        _dictionarySourceLazy;

    private readonly Lazy<LocalizedTextServiceFileSources>? _fileSources;
    private readonly ILogger<LocalizedTextService> _logger;

    private readonly Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, string>>>> _noAreaDictionarySourceLazy;

    /// <summary>
    ///     Initializes with a file sources instance
    /// </summary>
    /// <param name="fileSources"></param>
    /// <param name="logger"></param>
    public LocalizedTextService(
        Lazy<LocalizedTextServiceFileSources> fileSources,
        ILogger<LocalizedTextService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (fileSources == null)
        {
            throw new ArgumentNullException(nameof(fileSources));
        }

        _dictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>>(() =>
                FileSourcesToAreaDictionarySources(fileSources.Value));
        _noAreaDictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, string>>>>(() =>
                FileSourcesToNoAreaDictionarySources(fileSources.Value));
        _fileSources = fileSources;
    }

    /// <summary>
    ///     Initializes with an XML source
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    public LocalizedTextService(
        IDictionary<CultureInfo, Lazy<XDocument>> source,
        ILogger<LocalizedTextService> logger)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _dictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>>(() =>
                XmlSourcesToAreaDictionary(source));
        _noAreaDictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, string>>>>(() =>
                XmlSourceToNoAreaDictionary(source));
    }

    [Obsolete(
        "Use other ctor with IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> as input parameter.")]
    public LocalizedTextService(
        IDictionary<CultureInfo, IDictionary<string, IDictionary<string, string>>> source,
        ILogger<LocalizedTextService> logger)
        : this(
        source.ToDictionary(x => x.Key, x => new Lazy<IDictionary<string, IDictionary<string, string>>>(() => x.Value)),
        logger)
    {
    }

    /// <summary>
    ///     Initializes with a source of a dictionary of culture -> areas -> sub dictionary of keys/values
    /// </summary>
    /// <param name="source"></param>
    /// <param name="logger"></param>
    public LocalizedTextService(
        IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> source,
        ILogger<LocalizedTextService> logger)
    {
        IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> dictionarySource =
            source ?? throw new ArgumentNullException(nameof(source));
        _dictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>>(() =>
                dictionarySource);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var cultureNoAreaDictionary = new Dictionary<CultureInfo, Lazy<IDictionary<string, string>>>();
        foreach (KeyValuePair<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> cultureDictionary in
                 dictionarySource)
        {
            Dictionary<string, IDictionary<string, string>> areaAliaValue =
                GetAreaStoredTranslations(source, cultureDictionary.Key);

            cultureNoAreaDictionary.Add(
                cultureDictionary.Key,
                new Lazy<IDictionary<string, string>>(() => GetAliasValues(areaAliaValue)));
        }

        _noAreaDictionarySourceLazy =
            new Lazy<IDictionary<CultureInfo, Lazy<IDictionary<string, string>>>>(() => cultureNoAreaDictionary);
    }

    private IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> DictionarySource =>
        _dictionarySourceLazy.Value;

    private IDictionary<CultureInfo, Lazy<IDictionary<string, string>>> NoAreaDictionarySource =>
        _noAreaDictionarySourceLazy.Value;

    public string Localize(string? area, string? alias, CultureInfo? culture, IDictionary<string, string?>? tokens = null)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        // This is what the legacy ui service did
        if (string.IsNullOrEmpty(alias))
        {
            return string.Empty;
        }

        // TODO: Hack, see notes on ConvertToSupportedCultureWithRegionCode
        culture = ConvertToSupportedCultureWithRegionCode(culture);

        return GetFromDictionarySource(culture, area, alias, tokens);
    }

    /// <summary>
    ///     Returns all key/values in storage for the given culture
    /// </summary>
    public IDictionary<string, string> GetAllStoredValues(CultureInfo culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        // TODO: Hack, see notes on ConvertToSupportedCultureWithRegionCode
        culture = ConvertToSupportedCultureWithRegionCode(culture);

        if (DictionarySource.ContainsKey(culture) == false)
        {
            _logger.LogWarning(
                "The culture specified {Culture} was not found in any configured sources for this service",
                culture);
            return new Dictionary<string, string>(0);
        }

        IDictionary<string, string> result = new Dictionary<string, string>();

        // convert all areas + keys to a single key with a '/'
        foreach (KeyValuePair<string, IDictionary<string, string>> area in DictionarySource[culture].Value)
        {
            foreach (KeyValuePair<string, string> key in area.Value)
            {
                var dictionaryKey = string.Format("{0}/{1}", area.Key, key.Key);

                // i don't think it's possible to have duplicates because we're dealing with a dictionary in the first place, but we'll double check here just in case.
                if (result.ContainsKey(dictionaryKey) == false)
                {
                    result.Add(dictionaryKey, key.Value);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Returns a list of all currently supported cultures
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CultureInfo> GetSupportedCultures() => DictionarySource.Keys;

    /// <summary>
    ///     Tries to resolve a full 4 letter culture from a 2 letter culture name
    /// </summary>
    /// <param name="currentCulture">
    ///     The culture to determine if it is only a 2 letter culture, if so we'll try to convert it, otherwise it will just be
    ///     returned
    /// </param>
    /// <returns></returns>
    /// <remarks>
    ///     TODO: This is just a hack due to the way we store the language files, they should be stored with 4 letters since
    ///     that
    ///     is what they reference but they are stored with 2, further more our user's languages are stored with 2. So this
    ///     attempts
    ///     to resolve the full culture if possible.
    ///     This only works when this service is constructed with the LocalizedTextServiceFileSources
    /// </remarks>
    public CultureInfo ConvertToSupportedCultureWithRegionCode(CultureInfo currentCulture)
    {
        if (currentCulture == null)
        {
            throw new ArgumentNullException("currentCulture");
        }

        if (_fileSources == null)
        {
            return currentCulture;
        }

        if (currentCulture.Name.Length > 2)
        {
            return currentCulture;
        }

        Attempt<CultureInfo?> attempt =
            _fileSources.Value.TryConvert2LetterCultureTo4Letter(currentCulture.TwoLetterISOLanguageName);
        return attempt.Success ? attempt.Result! : currentCulture;
    }

    /// <summary>
    ///     Returns all key/values in storage for the given culture
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, IDictionary<string, string>> GetAllStoredValuesByAreaAndAlias(CultureInfo culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException("culture");
        }

        // TODO: Hack, see notes on ConvertToSupportedCultureWithRegionCode
        culture = ConvertToSupportedCultureWithRegionCode(culture);

        if (DictionarySource.ContainsKey(culture) == false)
        {
            _logger.LogWarning(
                "The culture specified {Culture} was not found in any configured sources for this service",
                culture);
            return new Dictionary<string, IDictionary<string, string>>(0);
        }

        return DictionarySource[culture].Value;
    }

    public string Localize(string key, CultureInfo culture, IDictionary<string, string?>? tokens = null)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        // This is what the legacy ui service did
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        var keyParts = key.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);
        var area = keyParts.Length > 1 ? keyParts[0] : null;
        var alias = keyParts.Length > 1 ? keyParts[1] : keyParts[0];
        return Localize(area, alias, culture, tokens);
    }

    /// <summary>
    ///     Parses the tokens in the value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is based on how the legacy ui localized text worked, each token was just a sequential value delimited with a %
    ///     symbol.
    ///     For example: hello %0%, you are %1% !
    ///     Since we're going to continue using the same language files for now, the token system needs to remain the same.
    ///     With our new service
    ///     we support a dictionary which means in the future we can really have any sort of token system.
    ///     Currently though, the token key's will need to be an integer and sequential - though we aren't going to throw
    ///     exceptions if that is not the case.
    /// </remarks>
    internal static string ParseTokens(string value, IDictionary<string, string?>? tokens)
    {
        if (tokens == null || tokens.Any() == false)
        {
            return value;
        }

        foreach (KeyValuePair<string, string?> token in tokens)
        {
            value = value.Replace(string.Concat("%", token.Key, "%"), token.Value);
        }

        return value;
    }

    private static Dictionary<string, string> GetAliasValues(
        Dictionary<string, IDictionary<string, string>> areaAliaValue)
    {
        var aliasValue = new Dictionary<string, string>();
        foreach (KeyValuePair<string, IDictionary<string, string>> area in areaAliaValue)
        {
            foreach (KeyValuePair<string, string> alias in area.Value)
            {
                if (!aliasValue.ContainsKey(alias.Key))
                {
                    aliasValue.Add(alias.Key, alias.Value);
                }
            }
        }

        return aliasValue;
    }

    private IDictionary<CultureInfo, Lazy<IDictionary<string, string>>> FileSourcesToNoAreaDictionarySources(
        LocalizedTextServiceFileSources fileSources)
    {
        IDictionary<CultureInfo, Lazy<XDocument>> xmlSources = fileSources.GetXmlSources();

        return XmlSourceToNoAreaDictionary(xmlSources);
    }

    private IDictionary<CultureInfo, Lazy<IDictionary<string, string>>> XmlSourceToNoAreaDictionary(
        IDictionary<CultureInfo, Lazy<XDocument>> xmlSources)
    {
        var cultureNoAreaDictionary = new Dictionary<CultureInfo, Lazy<IDictionary<string, string>>>();
        foreach (KeyValuePair<CultureInfo, Lazy<XDocument>> xmlSource in xmlSources)
        {
            var noAreaAliasValue =
                new Lazy<IDictionary<string, string>>(() => GetNoAreaStoredTranslations(xmlSources, xmlSource.Key));
            cultureNoAreaDictionary.Add(xmlSource.Key, noAreaAliasValue);
        }

        return cultureNoAreaDictionary;
    }

    private IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
        FileSourcesToAreaDictionarySources(LocalizedTextServiceFileSources fileSources)
    {
        IDictionary<CultureInfo, Lazy<XDocument>> xmlSources = fileSources.GetXmlSources();
        return XmlSourcesToAreaDictionary(xmlSources);
    }

    private IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
        XmlSourcesToAreaDictionary(IDictionary<CultureInfo, Lazy<XDocument>> xmlSources)
    {
        var cultureDictionary =
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>();
        foreach (KeyValuePair<CultureInfo, Lazy<XDocument>> xmlSource in xmlSources)
        {
            var areaAliaValue =
                new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                    GetAreaStoredTranslations(xmlSources, xmlSource.Key));
            cultureDictionary.Add(xmlSource.Key, areaAliaValue);
        }

        return cultureDictionary;
    }

    private IDictionary<string, IDictionary<string, string>> GetAreaStoredTranslations(
        IDictionary<CultureInfo, Lazy<XDocument>> xmlSource, CultureInfo cult)
    {
        var overallResult = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCulture);
        IEnumerable<XElement> areas = xmlSource[cult].Value.XPathSelectElements("//area");
        foreach (XElement area in areas)
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCulture);
            IEnumerable<XElement> keys = area.XPathSelectElements("./key");
            foreach (XElement key in keys)
            {
                var dictionaryKey =
                    (string)key.Attribute("alias")!;

                // there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                if (result.ContainsKey(dictionaryKey) == false)
                {
                    result.Add(dictionaryKey, key.Value);
                }
            }

            overallResult.Add(area.Attribute("alias")!.Value, result);
        }

        // Merge English Dictionary
        var englishCulture = new CultureInfo("en-US");
        if (!cult.Equals(englishCulture))
        {
            IEnumerable<XElement> enUS = xmlSource[englishCulture].Value.XPathSelectElements("//area");
            foreach (XElement area in enUS)
            {
                IDictionary<string, string>
                    result = new Dictionary<string, string>(StringComparer.InvariantCulture);
                if (overallResult.ContainsKey(area.Attribute("alias")!.Value))
                {
                    result = overallResult[area.Attribute("alias")!.Value];
                }

                IEnumerable<XElement> keys = area.XPathSelectElements("./key");
                foreach (XElement key in keys)
                {
                    var dictionaryKey =
                        (string)key.Attribute("alias")!;

                    // there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                    if (result.ContainsKey(dictionaryKey) == false)
                    {
                        result.Add(dictionaryKey, key.Value);
                    }
                }

                if (!overallResult.ContainsKey(area.Attribute("alias")!.Value))
                {
                    overallResult.Add(area.Attribute("alias")!.Value, result);
                }
            }
        }

        return overallResult;
    }

    private Dictionary<string, string> GetNoAreaStoredTranslations(
        IDictionary<CultureInfo, Lazy<XDocument>> xmlSource, CultureInfo cult)
    {
        var result = new Dictionary<string, string>(StringComparer.InvariantCulture);
        IEnumerable<XElement> keys = xmlSource[cult].Value.XPathSelectElements("//key");

        foreach (XElement key in keys)
        {
            var dictionaryKey =
                (string)key.Attribute("alias")!;

            // there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
            if (result.ContainsKey(dictionaryKey) == false)
            {
                result.Add(dictionaryKey, key.Value);
            }
        }

        // Merge English Dictionary
        var englishCulture = new CultureInfo("en-US");
        if (!cult.Equals(englishCulture))
        {
            IEnumerable<XElement> keysEn = xmlSource[englishCulture].Value.XPathSelectElements("//key");

            foreach (XElement key in keys)
            {
                var dictionaryKey =
                    (string)key.Attribute("alias")!;

                // there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                if (result.ContainsKey(dictionaryKey) == false)
                {
                    result.Add(dictionaryKey, key.Value);
                }
            }
        }

        return result;
    }

    private Dictionary<string, IDictionary<string, string>> GetAreaStoredTranslations(
        IDictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>> dictionarySource,
        CultureInfo cult)
    {
        var overallResult = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCulture);
        Lazy<IDictionary<string, IDictionary<string, string>>> areaDict = dictionarySource[cult];

        foreach (KeyValuePair<string, IDictionary<string, string>> area in areaDict.Value)
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCulture);
            ICollection<string> keys = area.Value.Keys;
            foreach (var key in keys)
            {
                // there could be duplicates if the language file isn't formatted nicely - which is probably the case for quite a few lang files
                if (result.ContainsKey(key) == false)
                {
                    result.Add(key, area.Value[key]);
                }
            }

            overallResult.Add(area.Key, result);
        }

        return overallResult;
    }

    private string GetFromDictionarySource(CultureInfo culture, string? area, string key, IDictionary<string, string?>? tokens)
    {
        if (DictionarySource.ContainsKey(culture) == false)
        {
            _logger.LogWarning(
                "The culture specified {Culture} was not found in any configured sources for this service",
                culture);
            return "[" + key + "]";
        }

        string? found = null;
        if (string.IsNullOrWhiteSpace(area))
        {
            NoAreaDictionarySource[culture].Value.TryGetValue(key, out found);
        }
        else
        {
            if (DictionarySource[culture].Value.TryGetValue(area, out IDictionary<string, string>? areaDictionary))
            {
                areaDictionary.TryGetValue(key, out found);
            }

            if (found == null)
            {
                NoAreaDictionarySource[culture].Value.TryGetValue(key, out found);
            }
        }

        if (found != null)
        {
            return ParseTokens(found, tokens);
        }

        // NOTE: Based on how legacy works, the default text does not contain the area, just the key
        return "[" + key + "]";
    }
}
