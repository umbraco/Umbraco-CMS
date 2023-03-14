using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings;

public class DefaultShortStringHelperConfig
{
    private readonly Dictionary<string, Dictionary<CleanStringType, Config>> _configs = new();

    public string DefaultCulture { get; set; } = string.Empty; // invariant

    public Dictionary<string, string>? UrlReplaceCharacters { get; set; }

    public DefaultShortStringHelperConfig Clone()
    {
        var config = new DefaultShortStringHelperConfig
        {
            DefaultCulture = DefaultCulture,
            UrlReplaceCharacters = UrlReplaceCharacters,
        };

        foreach (KeyValuePair<string, Dictionary<CleanStringType, Config>> kvp1 in _configs)
        {
            Dictionary<CleanStringType, Config> c = config._configs[kvp1.Key] =
                new Dictionary<CleanStringType, Config>();
            foreach (KeyValuePair<CleanStringType, Config> kvp2 in _configs[kvp1.Key])
            {
                c[kvp2.Key] = kvp2.Value.Clone();
            }
        }

        return config;
    }

    public DefaultShortStringHelperConfig WithConfig(Config config) =>
        WithConfig(DefaultCulture, CleanStringType.RoleMask, config);

    public DefaultShortStringHelperConfig WithConfig(CleanStringType stringRole, Config config) =>
        WithConfig(DefaultCulture, stringRole, config);

    public DefaultShortStringHelperConfig WithConfig(string? culture, CleanStringType stringRole, Config config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        culture = culture ?? string.Empty;

        if (_configs.ContainsKey(culture) == false)
        {
            _configs[culture] = new Dictionary<CleanStringType, Config>();
        }

        _configs[culture][stringRole] = config;
        return this;
    }

    /// <summary>
    ///     Sets the default configuration.
    /// </summary>
    /// <returns>The short string helper.</returns>
    public DefaultShortStringHelperConfig WithDefault(RequestHandlerSettings requestHandlerSettings)
    {
        IEnumerable<IChar> charCollection = requestHandlerSettings.GetCharReplacements();

        UrlReplaceCharacters = charCollection
            .Where(x => string.IsNullOrEmpty(x.Char) == false)
            .ToDictionary(x => x.Char, x => x.Replacement);

        CleanStringType urlSegmentConvertTo = CleanStringType.Utf8;
        if (requestHandlerSettings.ShouldConvertUrlsToAscii)
        {
            urlSegmentConvertTo = CleanStringType.Ascii;
        }

        if (requestHandlerSettings.ShouldTryConvertUrlsToAscii)
        {
            urlSegmentConvertTo = CleanStringType.TryAscii;
        }

        return WithConfig(CleanStringType.UrlSegment, new Config
        {
            PreFilter = ApplyUrlReplaceCharacters,
            PostFilter = x => CutMaxLength(x, 240),
            IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
            StringType = urlSegmentConvertTo | CleanStringType.LowerCase,
            BreakTermsOnUpper = false,
            Separator = '-',
        }).WithConfig(CleanStringType.FileName, new Config
        {
            PreFilter = ApplyUrlReplaceCharacters,
            IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
            StringType = CleanStringType.Utf8 | CleanStringType.LowerCase,
            BreakTermsOnUpper = false,
            Separator = '-',
        }).WithConfig(CleanStringType.Alias, new Config
        {
            PreFilter = ApplyUrlReplaceCharacters,
            IsTerm = (c, leading) => leading
                ? char.IsLetter(c) // only letters
                : char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
            StringType = CleanStringType.Ascii | CleanStringType.UmbracoCase,
            BreakTermsOnUpper = false,
        }).WithConfig(CleanStringType.UnderscoreAlias, new Config
        {
            PreFilter = ApplyUrlReplaceCharacters,
            IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
            StringType = CleanStringType.Ascii | CleanStringType.UmbracoCase,
            BreakTermsOnUpper = false,
        }).WithConfig(CleanStringType.ConvertCase, new Config
        {
            PreFilter = null,
            IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
            StringType = CleanStringType.Ascii,
            BreakTermsOnUpper = true,
        });
    }

    // internal: we don't want ppl to retrieve a config and modify it
    // (the helper uses a private clone to prevent modifications)
    internal Config For(CleanStringType stringType, string? culture)
    {
        culture = culture ?? string.Empty;
        stringType = stringType & CleanStringType.RoleMask;

        Dictionary<CleanStringType, Config> config;
        if (_configs.ContainsKey(culture))
        {
            config = _configs[culture];

            // have we got a config for _that_ role?
            if (config.ContainsKey(stringType))
            {
                return config[stringType];
            }

            // have we got a generic config for _all_ roles?
            if (config.ContainsKey(CleanStringType.RoleMask))
            {
                return config[CleanStringType.RoleMask];
            }
        }
        else if (_configs.ContainsKey(DefaultCulture))
        {
            config = _configs[DefaultCulture];

            // have we got a config for _that_ role?
            if (config.ContainsKey(stringType))
            {
                return config[stringType];
            }

            // have we got a generic config for _all_ roles?
            if (config.ContainsKey(CleanStringType.RoleMask))
            {
                return config[CleanStringType.RoleMask];
            }
        }

        return Config.NotConfigured;
    }

    /// <summary>
    ///     Returns a new string in which characters have been replaced according to the Umbraco settings UrlReplaceCharacters.
    /// </summary>
    /// <param name="s">The string to filter.</param>
    /// <returns>The filtered string.</returns>
    public string ApplyUrlReplaceCharacters(string s) =>
        UrlReplaceCharacters == null ? s : s.ReplaceMany(UrlReplaceCharacters);

    public static string CutMaxLength(string text, int length) =>
        text.Length <= length ? text : text.Substring(0, length);

    public sealed class Config
    {
        internal static readonly Config NotConfigured = new();

        public Config()
        {
            StringType = CleanStringType.Utf8 | CleanStringType.Unchanged;
            PreFilter = null;
            PostFilter = null;
            IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c);
            BreakTermsOnUpper = false;
            CutAcronymOnNonUpper = false;
            GreedyAcronyms = false;
            Separator = char.MinValue;
        }

        public Func<string, string>? PreFilter { get; set; }

        public Func<string, string>? PostFilter { get; set; }

        public Func<char, bool, bool> IsTerm { get; set; }

        public CleanStringType StringType { get; set; }

        // indicate whether an uppercase within a term eg "fooBar" is to break
        // into a new term, or to be considered as part of the current term
        public bool BreakTermsOnUpper { get; set; }

        // indicate whether a non-uppercase within an acronym eg "FOOBar" is to cut
        // the acronym (at "B" or "a" depending on GreedyAcronyms) or to give
        // up the acronym and treat the term as a word
        public bool CutAcronymOnNonUpper { get; set; }

        // indicates whether acronyms parsing is greedy ie whether "FOObar" is
        // "FOO" + "bar" (greedy) or "FO" + "Obar" (non-greedy)
        public bool GreedyAcronyms { get; set; }

        // the separator char
        // but then how can we tell we don't want any?
        public char Separator { get; set; }

        public Config Clone() =>
            new Config
            {
                PreFilter = PreFilter,
                PostFilter = PostFilter,
                IsTerm = IsTerm,
                StringType = StringType,
                BreakTermsOnUpper = BreakTermsOnUpper,
                CutAcronymOnNonUpper = CutAcronymOnNonUpper,
                GreedyAcronyms = GreedyAcronyms,
                Separator = Separator,
            };

        // extends the config
        public CleanStringType StringTypeExtend(CleanStringType stringType)
        {
            CleanStringType st = StringType;
            foreach (CleanStringType mask in new[] { CleanStringType.CaseMask, CleanStringType.CodeMask })
            {
                CleanStringType a = stringType & mask;
                if (a == 0)
                {
                    continue;
                }

                st = st & ~mask; // clear what we have
                st = st | a; // set the new value
            }

            return st;
        }
    }
}
