using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Provides configuration for the <see cref="DefaultShortStringHelper"/>.
/// </summary>
/// <remarks>
///     This class manages culture-specific and string-type-specific configurations
///     for cleaning and transforming strings.
/// </remarks>
public class DefaultShortStringHelperConfig
{
    /// <summary>
    ///     The dictionary of configurations per culture and string type.
    /// </summary>
    private readonly Dictionary<string, Dictionary<CleanStringType, Config>> _configs = new();

    /// <summary>
    ///     Gets or sets the default culture to use for string operations.
    /// </summary>
    public string DefaultCulture { get; set; } = string.Empty; // invariant

    /// <summary>
    ///     Gets or sets the dictionary of character replacements for URL generation.
    /// </summary>
    public Dictionary<string, string>? UrlReplaceCharacters { get; set; }

    /// <summary>
    ///     Creates a deep copy of this configuration.
    /// </summary>
    /// <returns>A new <see cref="DefaultShortStringHelperConfig"/> instance with the same settings.</returns>
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

    /// <summary>
    ///     Adds or updates a configuration for the default culture and all string roles.
    /// </summary>
    /// <param name="config">The configuration to set.</param>
    /// <returns>This instance for method chaining.</returns>
    public DefaultShortStringHelperConfig WithConfig(Config config) =>
        WithConfig(DefaultCulture, CleanStringType.RoleMask, config);

    /// <summary>
    ///     Adds or updates a configuration for the default culture and a specific string role.
    /// </summary>
    /// <param name="stringRole">The string role to configure.</param>
    /// <param name="config">The configuration to set.</param>
    /// <returns>This instance for method chaining.</returns>
    public DefaultShortStringHelperConfig WithConfig(CleanStringType stringRole, Config config) =>
        WithConfig(DefaultCulture, stringRole, config);

    /// <summary>
    ///     Adds or updates a configuration for a specific culture and string role.
    /// </summary>
    /// <param name="culture">The culture to configure, or <c>null</c> for the default culture.</param>
    /// <param name="stringRole">The string role to configure.</param>
    /// <param name="config">The configuration to set.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    public DefaultShortStringHelperConfig WithConfig(string? culture, CleanStringType stringRole, Config config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        culture = culture ?? string.Empty;

        if (_configs.TryGetValue(culture, out Dictionary<CleanStringType, Config>? configForCulture) == false)
        {
            configForCulture = _configs[culture] = new Dictionary<CleanStringType, Config>();
        }

        configForCulture[stringRole] = config;
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
        else if (requestHandlerSettings.ShouldTryConvertUrlsToAscii)
        {
            urlSegmentConvertTo = CleanStringType.TryAscii;
        }

        CleanStringType fileNameSegmentConvertTo = CleanStringType.Utf8;
        if (requestHandlerSettings.ShouldConvertFileNamesToAscii)
        {
            fileNameSegmentConvertTo = CleanStringType.Ascii;
        }
        else if (requestHandlerSettings.ShouldTryConvertFileNamesToAscii)
        {
            fileNameSegmentConvertTo = CleanStringType.TryAscii;
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
            StringType = fileNameSegmentConvertTo | CleanStringType.LowerCase,
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

    /// <summary>
    ///     Gets the configuration for the specified string type and culture.
    /// </summary>
    /// <param name="stringType">The type of string cleaning.</param>
    /// <param name="culture">The culture name.</param>
    /// <returns>The configuration for the specified string type and culture.</returns>
    /// <remarks>
    ///     Internal: we don't want people to retrieve a config and modify it.
    ///     The helper uses a private clone to prevent modifications.
    /// </remarks>
    internal Config For(CleanStringType stringType, string? culture)
    {
        culture = culture ?? string.Empty;
        stringType = stringType & CleanStringType.RoleMask;

        if (_configs.TryGetValue(culture, out Dictionary<CleanStringType, Config>? configForCulture))
        {
            // have we got a config for _that_ role?
            if (configForCulture.TryGetValue(stringType, out Config? configForStringType))
            {
                return configForStringType;
            }

            // have we got a generic config for _all_ roles?
            if (configForCulture.TryGetValue(CleanStringType.RoleMask, out Config? configForRoleMask))
            {
                return configForRoleMask;
            }
        }
        else if (_configs.TryGetValue(DefaultCulture, out Dictionary<CleanStringType, Config>? configForDefaultCulture))
        {
            // have we got a config for _that_ role?
            if (configForDefaultCulture.TryGetValue(stringType, out Config? configForStringType))
            {
                return configForStringType;
            }

            // have we got a generic config for _all_ roles?
            if (configForDefaultCulture.TryGetValue(CleanStringType.RoleMask, out Config? configForRoleMask))
            {
                return configForRoleMask;
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

    /// <summary>
    ///     Cuts a string to a maximum length.
    /// </summary>
    /// <param name="text">The text to cut.</param>
    /// <param name="length">The maximum length.</param>
    /// <returns>The original text if shorter than or equal to <paramref name="length"/>; otherwise, the first <paramref name="length"/> characters.</returns>
    public static string CutMaxLength(string text, int length) =>
        text.Length <= length ? text : text.Substring(0, length);

    /// <summary>
    ///     Represents configuration settings for a specific string type and culture combination.
    /// </summary>
    /// <summary>
    ///     Represents configuration settings for a specific string type and culture combination.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        ///     The default configuration instance returned when no configuration is found.
        /// </summary>
        internal static readonly Config NotConfigured = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Config"/> class with default settings.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets a function to apply before string processing.
        /// </summary>
        public Func<string, string>? PreFilter { get; set; }

        /// <summary>
        ///     Gets or sets a function to apply after string processing.
        /// </summary>
        public Func<string, string>? PostFilter { get; set; }

        /// <summary>
        ///     Gets or sets a function that determines whether a character is a valid term character.
        /// </summary>
        /// <remarks>
        ///     The first parameter is the character, the second indicates whether it is a leading character.
        /// </remarks>
        public Func<char, bool, bool> IsTerm { get; set; }

        /// <summary>
        ///     Gets or sets the target string type (casing and encoding).
        /// </summary>
        public CleanStringType StringType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether an uppercase character within a term (e.g., "fooBar")
        ///     should break into a new term, or be considered as part of the current term.
        /// </summary>
        public bool BreakTermsOnUpper { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a non-uppercase character within an acronym (e.g., "FOOBar")
        ///     should cut the acronym or give up the acronym and treat the term as a word.
        /// </summary>
        public bool CutAcronymOnNonUpper { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether acronym parsing is greedy,
        ///     i.e., whether "FOObar" is "FOO" + "bar" (greedy) or "FO" + "Obar" (non-greedy).
        /// </summary>
        public bool GreedyAcronyms { get; set; }

        /// <summary>
        ///     Gets or sets the separator character to use between terms.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="char.MinValue"/> to indicate no separator.
        /// </remarks>
        public char Separator { get; set; }

        /// <summary>
        ///     Creates a deep copy of this configuration.
        /// </summary>
        /// <returns>A new <see cref="Config"/> instance with the same settings.</returns>
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

        /// <summary>
        ///     Extends the configuration's string type with values from the specified string type.
        /// </summary>
        /// <param name="stringType">The string type to merge into this configuration.</param>
        /// <returns>The merged string type.</returns>
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
