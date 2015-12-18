using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// New default implementation of string functions for short strings such as aliases or url segments.
    /// </summary>
    /// <remarks>
    /// <para>Not optimized to work on large bodies of text.</para>
    /// <para>Meant to replace <c>LegacyShortStringHelper</c> where/when backward compatibility is not an issue.</para>
    /// <para>NOTE: pre-filters run _before_ the string is re-encoded.</para>
    /// </remarks>
    public class DefaultShortStringHelper : IShortStringHelper
    {
        private readonly IUmbracoSettingsSection _umbracoSettings;

        #region Ctor and vars

        [Obsolete("Use the other ctor that specifies all dependencies")]
        public DefaultShortStringHelper()
        {
            _umbracoSettings = _umbracoSettings;
            InitializeLegacyUrlReplaceCharacters();
        }

        public DefaultShortStringHelper(IUmbracoSettingsSection umbracoSettings)
        {
            _umbracoSettings = umbracoSettings;
            InitializeLegacyUrlReplaceCharacters();
        }

        /// <summary>
        /// Freezes the helper so it can prevents its configuration from being modified.
        /// </summary>
        /// <remarks>Will be called by <c>ShortStringHelperResolver</c> when resolution freezes.</remarks>
        public void Freeze()
        {
            _frozen = true;
        }

        // see notes for CleanAsciiString
        //// beware! the order is quite important here!
        //const string ValidStringCharactersSource = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //readonly static char[] ValidStringCharacters;

        private CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
        private bool _frozen;
        private readonly Dictionary<CultureInfo, Dictionary<CleanStringType, Config>> _configs = new Dictionary<CultureInfo, Dictionary<CleanStringType, Config>>();

        // see notes for CleanAsciiString
        //static DefaultShortStringHelper()
        //{
        //    ValidStringCharacters = ValidStringCharactersSource.ToCharArray();
        //}

        #endregion

        #region Filters

        private readonly Dictionary<string, string> _urlReplaceCharacters = new Dictionary<string, string>();

        private void InitializeLegacyUrlReplaceCharacters()
        {
            foreach (var node in _umbracoSettings.RequestHandler.CharCollection)
            {
                if(string.IsNullOrEmpty(node.Char) == false)
                    _urlReplaceCharacters[node.Char] = node.Replacement;
            }
        }

        /// <summary>
        /// Returns a new string in which characters have been replaced according to the Umbraco settings UrlReplaceCharacters.
        /// </summary>
        /// <param name="s">The string to filter.</param>
        /// <returns>The filtered string.</returns>
        public string ApplyUrlReplaceCharacters(string s)
        {
            return s.ReplaceMany(_urlReplaceCharacters);
        }

        // ok to be static here because it's not configureable in any way
        private static readonly char[] InvalidFileNameChars =
            Path.GetInvalidFileNameChars()
            .Union("!*'();:@&=+$,/?%#[]-~{}\"<>\\^`| ".ToCharArray())
            .Distinct()
            .ToArray();

        public static bool IsValidFileNameChar(char c)
        {
            return InvalidFileNameChars.Contains(c) == false;
        }

        public static string CutMaxLength(string text, int length)
        {
            return text.Length <= length ? text : text.Substring(0, length);
        }

        #endregion

        #region Configuration

        private void EnsureNotFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException("Cannot configure the helper once it is frozen.");            
        }

        /// <summary>
        /// Sets a default culture.
        /// </summary>
        /// <param name="culture">The default culture.</param>
        /// <returns>The short string helper.</returns>
        public DefaultShortStringHelper WithDefaultCulture(CultureInfo culture)
        {
            EnsureNotFrozen();
            _defaultCulture = culture;
            return this;
        }

        public DefaultShortStringHelper WithConfig(Config config)
        {
            return WithConfig(_defaultCulture, CleanStringType.RoleMask, config);
        }

        public DefaultShortStringHelper WithConfig(CleanStringType stringRole, Config config)
        {
            return WithConfig(_defaultCulture, stringRole, config);
        }

        public DefaultShortStringHelper WithConfig(CultureInfo culture, CleanStringType stringRole, Config config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            EnsureNotFrozen();
            if (_configs.ContainsKey(culture) == false)
                _configs[culture] = new Dictionary<CleanStringType, Config>();
            _configs[culture][stringRole] = config.Clone(); // clone so it can't be changed
            return this;
        }

        /// <summary>
        /// Sets the default configuration.
        /// </summary>
        /// <returns>The short string helper.</returns>
        public DefaultShortStringHelper WithDefaultConfig()
        {
            return WithConfig(CleanStringType.UrlSegment, new Config
            {
                PreFilter = ApplyUrlReplaceCharacters,
                PostFilter = x => CutMaxLength(x, 240),
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                StringType = (_umbracoSettings.RequestHandler.ConvertUrlsToAscii ? CleanStringType.Ascii : CleanStringType.Utf8) | CleanStringType.LowerCase,
                BreakTermsOnUpper = false,
                Separator = '-'
            }).WithConfig(CleanStringType.FileName, new Config
            {
                PreFilter = ApplyUrlReplaceCharacters,
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                StringType = CleanStringType.Utf8 | CleanStringType.LowerCase,
                BreakTermsOnUpper = false,
                Separator = '-'
            }).WithConfig(CleanStringType.Alias, new Config
            {
                PreFilter = ApplyUrlReplaceCharacters,
                IsTerm = (c, leading) => leading 
                    ? char.IsLetter(c) // only letters
                    : (char.IsLetterOrDigit(c) || c == '_'), // letter, digit or underscore
                StringType = CleanStringType.Ascii | CleanStringType.UmbracoCase,
                BreakTermsOnUpper = false
            }).WithConfig(CleanStringType.UnderscoreAlias, new Config
            {
                PreFilter = ApplyUrlReplaceCharacters,
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                StringType = CleanStringType.Ascii | CleanStringType.UmbracoCase,
                BreakTermsOnUpper = false
            }).WithConfig(CleanStringType.ConvertCase, new Config
            {
                PreFilter = null,
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                StringType = CleanStringType.Ascii,
                BreakTermsOnUpper = true
            });
        }

        public sealed class Config
        {
            public Config()
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged;
                PreFilter = null;
                PostFilter = null;
                IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c);
                BreakTermsOnUpper = false;
                CutAcronymOnNonUpper = false;
                GreedyAcronyms = false;
                Separator = Char.MinValue;
            }

            public Config Clone()
            {
                return new Config
                {
                    PreFilter = PreFilter,
                    PostFilter =  PostFilter,
                    IsTerm = IsTerm,
                    StringType = StringType,
                    BreakTermsOnUpper = BreakTermsOnUpper,
                    CutAcronymOnNonUpper =  CutAcronymOnNonUpper,
                    GreedyAcronyms =  GreedyAcronyms,
                    Separator = Separator
                };
            }

            public Func<string, string> PreFilter { get; set; }
            public Func<string, string> PostFilter { get; set; }
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
            // but then how can we tell we dont want any?
            public char Separator { get; set; }

            // extends the config
            public CleanStringType StringTypeExtend(CleanStringType stringType)
            {
                var st = StringType;
                foreach (var mask in new[] { CleanStringType.CaseMask, CleanStringType.CodeMask })
                {
                    var a = stringType & mask;
                    if (a == 0) continue;

                    st = st & ~mask; // clear what we have
                    st = st | a; // set the new value
                }
                return st;
            }

            internal static readonly Config NotConfigured = new Config();
        }

        private Config GetConfig(CleanStringType stringType, CultureInfo culture)
        {
            stringType = stringType & CleanStringType.RoleMask;

            Dictionary<CleanStringType, Config> config;
            if (_configs.ContainsKey(culture))
            {
                config = _configs[culture];
                if (config.ContainsKey(stringType)) // have we got a config for _that_ role?
                    return config[stringType];
                if (config.ContainsKey(CleanStringType.RoleMask)) // have we got a generic config for _all_ roles?
                    return config[CleanStringType.RoleMask];
            }
            else if (_configs.ContainsKey(_defaultCulture))
            {
                config = _configs[_defaultCulture];
                if (config.ContainsKey(stringType)) // have we got a config for _that_ role?
                    return config[stringType];
                if (config.ContainsKey(CleanStringType.RoleMask)) // have we got a generic config for _all_ roles?
                    return config[CleanStringType.RoleMask];
            }

            return Config.NotConfigured;
        }

        #endregion

        #region JavaScript

        private const string SssjsFormat = @"
var UMBRACO_FORCE_SAFE_ALIAS = {0};
var UMBRACO_FORCE_SAFE_ALIAS_URL = '{1}';
var UMBRACO_FORCE_SAFE_ALIAS_TIMEOUT = 666;
var UMBRACO_FORCE_SAFE_ALIAS_TMKEY = 'safe-alias-tmout';

function getSafeAliasFromServer(value, callback) {{
    $.getJSON(UMBRACO_FORCE_SAFE_ALIAS_URL + 'ToSafeAlias?value=' + encodeURIComponent(value), function(json) {{
        if (json.alias) {{ callback(json.alias); }}
    }});
}}

function getSafeAlias(input, value, immediate, callback) {{
    if (!UMBRACO_FORCE_SAFE_ALIAS) {{
        callback(value);
        return;
    }}
    var timeout = input.data(UMBRACO_FORCE_SAFE_ALIAS_TMKEY);
    if (timeout) clearTimeout(timeout);
    input.data(UMBRACO_FORCE_SAFE_ALIAS_TMKEY, setTimeout(function() {{
        input.removeData(UMBRACO_FORCE_SAFE_ALIAS_TMKEY);
        getSafeAliasFromServer(value, function(alias) {{ callback(alias); }});
    }}, UMBRACO_FORCE_SAFE_ALIAS_TIMEOUT));
}}

function validateSafeAlias(input, value, immediate, callback) {{
    if (!UMBRACO_FORCE_SAFE_ALIAS) {{
        callback(true);
        return;
    }}
    var timeout = input.data(UMBRACO_FORCE_SAFE_ALIAS_TMKEY);
    if (timeout) clearTimeout(timeout);
    input.data(UMBRACO_FORCE_SAFE_ALIAS_TMKEY, setTimeout(function() {{
        input.removeData(UMBRACO_FORCE_SAFE_ALIAS_TMKEY);
        getSafeAliasFromServer(value, function(alias) {{ callback(value.toLowerCase() == alias.toLowerCase()); }});
    }}, UMBRACO_FORCE_SAFE_ALIAS_TIMEOUT));
}}
";

        /// <summary>
        /// Gets the JavaScript code defining client-side short string services.
        /// </summary>
        public string GetShortStringServicesJavaScript(string controllerPath)
        {
                return string.Format(SssjsFormat,
                    _umbracoSettings.Content.ForceSafeAliases ? "true" : "false", controllerPath);
        }

        #endregion

        #region IShortStringHelper CleanFor...

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>
        /// <para>The string will be cleaned in the context of the default culture.</para>
        /// <para>Safe aliases are Ascii only.</para>
        /// </remarks>
        public virtual string CleanStringForSafeAlias(string text)
        {
            return CleanStringForSafeAlias(text, _defaultCulture);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe alias.</returns>
        /// <remarks>
        /// <para>Safe aliases are Ascii only.</para>
        /// </remarks>
        public virtual string CleanStringForSafeAlias(string text, CultureInfo culture)
        {
            return CleanString(text, CleanStringType.Alias, culture);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>
        /// <para>The string will be cleaned in the context of the default culture.</para>
        /// <para>Url segments are Ascii only (no accents...).</para>
        /// </remarks>
        public virtual string CleanStringForUrlSegment(string text)
        {
            return CleanStringForUrlSegment(text, _defaultCulture);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an url segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe url segment.</returns>
        /// <remarks>
        /// <para>Url segments are Ascii only (no accents...).</para>
        /// </remarks>
        public virtual string CleanStringForUrlSegment(string text, CultureInfo culture)
        {
            return CleanString(text, CleanStringType.UrlSegment, culture);
        }

        /// <summary>
        /// Cleans a string, in the context of the default culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but that issue is not documented.</remarks>
        public virtual string CleanStringForSafeFileName(string text)
        {
            return CleanStringForSafeFileName(text, _defaultCulture);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a url).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        public virtual string CleanStringForSafeFileName(string text, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ReplaceMany(Path.GetInvalidFileNameChars(), '-');

            var name = Path.GetFileNameWithoutExtension(text);
            var ext = Path.GetExtension(text); // includes the dot, empty if no extension

            Debug.Assert(name != null, "name != null");
            if (name.Length > 0)
                name = CleanString(name, CleanStringType.FileName, culture);
            Debug.Assert(ext != null, "ext != null");
            if (ext.Length > 0)
                ext = CleanString(ext.Substring(1), CleanStringType.FileName, culture);

            return ext.Length > 0 ? (name + "." + ext) : name;
        }

        #endregion

        #region CleanString

        // MS rules & guidelines:
        // - Do capitalize both characters of two-character acronyms, except the first word of a camel-cased identifier.
        //     eg "DBRate" (pascal) or "ioHelper" (camel) - "SpecialDBRate" (pascal) or "specialIOHelper" (camel)
        // - Do capitalize only the first character of acronyms with three or more characters, except the first word of a camel-cased identifier.
        //     eg "XmlWriter (pascal) or "htmlReader" (camel) - "SpecialXmlWriter" (pascal) or "specialHtmlReader" (camel)
        // - Do not capitalize any of the characters of any acronyms, whatever their length, at the beginning of a camel-cased identifier.
        //     eg "xmlWriter" or "dbWriter" (camel)
        //
        // Our additional stuff:
        // - Leading digits are removed.
        // - Many consecutive separators are folded into one unique separator.

        const byte StateBreak = 1;
        const byte StateUp = 2;
        const byte StateWord = 3;
        const byte StateAcronym = 4;

        /// <summary>
        /// Cleans a string.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the default culture.</remarks>
        public string CleanString(string text, CleanStringType stringType)
        {
            return CleanString(text, stringType, _defaultCulture, null);
        }

        /// <summary>
        /// Cleans a string, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the default culture.</remarks>
        public string CleanString(string text, CleanStringType stringType, char separator)
        {
            return CleanString(text, stringType, _defaultCulture, separator);
        }

        /// <summary>
        /// Cleans a string in the context of a specified culture.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public string CleanString(string text, CleanStringType stringType, CultureInfo culture)
        {
            return CleanString(text, stringType, culture, null);
        }

        /// <summary>
        /// Cleans a string in the context of a specified culture, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default, 
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public string CleanString(string text, CleanStringType stringType, char separator, CultureInfo culture)
        {
            return CleanString(text, stringType, culture, separator);
        }

        protected virtual string CleanString(string text, CleanStringType stringType, CultureInfo culture, char? separator)
        {
            // be safe
            if (text == null)
                throw new ArgumentNullException("text");
            if (culture == null)
                throw new ArgumentNullException("culture");

            // get config
            var config = GetConfig(stringType, culture);
            stringType = config.StringTypeExtend(stringType);

            // apply defaults
            if ((stringType & CleanStringType.CaseMask) == CleanStringType.None)
                stringType |= CleanStringType.CamelCase;
            if ((stringType & CleanStringType.CodeMask) == CleanStringType.None)
                stringType |= CleanStringType.Ascii;

            // use configured unless specified
            separator = separator ?? config.Separator;

            // apply pre-filter
            if (config.PreFilter != null)
                text = config.PreFilter(text);

            // apply replacements
            //if (config.Replacements != null)
            //    text = ReplaceMany(text, config.Replacements);

            // recode
            var codeType = stringType & CleanStringType.CodeMask;
            text = codeType == CleanStringType.Ascii 
                ? Utf8ToAsciiConverter.ToAsciiString(text) 
                : RemoveSurrogatePairs(text);

            // clean
            text = CleanCodeString(text, stringType, separator.Value, culture, config);

            // apply post-filter
            if (config.PostFilter != null)
                text = config.PostFilter(text);
            
            return text;
        }

        private static string RemoveSurrogatePairs(string text)
        {
            var input = text.ToCharArray();
            var output = new char[input.Length];
            var opos = 0;

            for (var ipos = 0; ipos < input.Length; ipos++)
            {
                var c = input[ipos];
                if (char.IsSurrogate(c)) // ignore high surrogate
                {
                    ipos++; // and skip low surrogate
                    output[opos++] = '?';
                }
                else
                {
                    output[opos++] = c;
                }
            }

            return new string(output, 0, opos);
        }

        // here was a subtle, ascii-optimized version of the cleaning code, and I was
        // very proud of it until benchmarking showed it was an order of magnitude slower
        // that the utf8 version. Micro-optimizing sometimes isn't such a good idea.

        // note: does NOT support surrogate pairs in text
        internal string CleanCodeString(string text, CleanStringType caseType, char separator, CultureInfo culture, Config config)
        {
            int opos = 0, ipos = 0;
            var state = StateBreak;

            caseType &= CleanStringType.CaseMask;

            // if we apply global ToUpper or ToLower to text here
            // then we cannot break words on uppercase chars
            var input = text;

            // it's faster to use an array than a StringBuilder
            var ilen = input.Length;
            var output = new char[ilen * 2]; // twice the length should be OK in all cases

            for (var i = 0; i < ilen; i++)
            {
                var c = input[i];
                // leading as long as StateBreak and ipos still zero
                var leading = state == StateBreak && ipos == 0;
                var isTerm = config.IsTerm(c, leading);

                //var isDigit = char.IsDigit(c);
                var isUpper = char.IsUpper(c); // false for digits, symbols...
                //var isLower = char.IsLower(c); // false for digits, symbols...

                // what should I do with surrogates?
                // no idea, really, so they are not supported at the moment
                var isPair = char.IsSurrogate(c);
                if (isPair)
                    throw new NotSupportedException("Surrogate pairs are not supported.");

                switch (state)
                {
                    // within a break
                    case StateBreak:
                        // begin a new term if char is a term char,
                        // and ( pos > 0 or it's also a valid leading char )
                        if (isTerm)
                        {
                            ipos = i;
                            if (opos > 0 && separator != char.MinValue)
                                output[opos++] = separator;
                            state = isUpper ? StateUp : StateWord;
                        }
                        break;

                    // within a term / word
                    case StateWord:
                        // end a term if char is not a term char,
                        // or ( it's uppercase and we break terms on uppercase)
                        if (isTerm == false || (config.BreakTermsOnUpper && isUpper))
                        {
                            CopyTerm(input, ipos, output, ref opos, i - ipos, caseType, culture, false);
                            ipos = i;
                            state = isTerm ? StateUp : StateBreak;
                            if (state != StateBreak && separator != char.MinValue)
                                output[opos++] = separator;
                        }
                        break;

                    // within a term / acronym
                    case StateAcronym:
                        // end an acronym if char is not a term char,
                        // or if it's not uppercase / config
                        if (isTerm == false || (config.CutAcronymOnNonUpper && isUpper == false))
                        {
                            // whether it's part of the acronym depends on whether we're greedy
                            if (isTerm && config.GreedyAcronyms == false)
                                i -= 1; // handle that char again, in another state - not part of the acronym
                            if (i - ipos > 1) // single-char can't be an acronym
                            {
                                CopyTerm(input, ipos, output, ref opos, i - ipos, caseType, culture, true);
                                ipos = i;
                                state = isTerm ? StateWord : StateBreak;
                                if (state != StateBreak && separator != char.MinValue)
                                    output[opos++] = separator;
                            }
                            else if (isTerm)
                            {
                                state = StateWord;
                            }
                        }
                        else if (isUpper == false) // isTerm == true
                        {
                            // it's a term char and we don't cut...
                            // keep moving forward as a word
                            state = StateWord;
                        }
                        break;

                    // within a term / uppercase = could be a word or an acronym
                    case StateUp:
                        if (isTerm)
                        {
                            // add that char to the term and pick word or acronym
                            state = isUpper ? StateAcronym : StateWord;
                        }
                        else
                        {
                            // single char, copy then break
                            CopyTerm(input, ipos, output, ref opos, 1, caseType, culture, false);
                            state = StateBreak;
                        }
                        break;

                    default:
                        throw new Exception("Invalid state.");
                }
            }

            switch (state)
            {
                case StateBreak:
                    break;

                case StateWord:
                    CopyTerm(input, ipos, output, ref opos, input.Length - ipos, caseType, culture, false);
                    break;

                case StateAcronym:
                case StateUp:
                    CopyTerm(input, ipos, output, ref opos, input.Length - ipos, caseType, culture, true);
                    break;

                default:
                    throw new Exception("Invalid state.");
            }

            return new string(output, 0, opos);
        }

        // note: supports surrogate pairs in input string
        internal void CopyTerm(string input, int ipos, char[] output, ref int opos, int len,
            CleanStringType caseType, CultureInfo culture, bool isAcronym)
        {
            var term = input.Substring(ipos, len);

            if (isAcronym)
            {
                if ((caseType == CleanStringType.CamelCase && len <= 2 && opos > 0) ||
                        (caseType == CleanStringType.PascalCase && len <= 2) ||
                        (caseType == CleanStringType.UmbracoCase))
                    caseType = CleanStringType.Unchanged;
            }

            // note: MSDN seems to imply that ToUpper or ToLower preserve the length
            // of the string, but that this behavior is not guaranteed and could change.

            char c;
            int i;
            string s;
            switch (caseType)
            {
                //case CleanStringType.LowerCase:
                //case CleanStringType.UpperCase:
                case CleanStringType.Unchanged:
                    term.CopyTo(0, output, opos, len);
                    opos += len;
                    break;

                case CleanStringType.LowerCase:
                    term = term.ToLower(culture);
                    term.CopyTo(0, output, opos, term.Length);
                    opos += term.Length;
                    break;

                case CleanStringType.UpperCase:
                    term = term.ToUpper(culture);
                    term.CopyTo(0, output, opos, term.Length);
                    opos += term.Length;
                    break;

                case CleanStringType.CamelCase:
                    c = term[0];
                    i = 1;
                    if (char.IsSurrogate(c))
                    {
                        s = term.Substring(ipos, 2);
                        s = opos == 0 ? s.ToLower(culture) : s.ToUpper(culture);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos] = opos++ == 0 ? char.ToLower(c, culture) : char.ToUpper(c, culture);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i).ToLower(culture);
                        term.CopyTo(0, output, opos, term.Length);
                        opos += term.Length;
                    }
                    break;

                case CleanStringType.PascalCase:
                    c = term[0];
                    i = 1;
                    if (char.IsSurrogate(c))
                    {
                        s = term.Substring(ipos, 2);
                        s = s.ToUpper(culture);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos++] = char.ToUpper(c, culture);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i).ToLower(culture);
                        term.CopyTo(0, output, opos, term.Length);
                        opos += term.Length;
                    }
                    break;

                case CleanStringType.UmbracoCase:
                    c = term[0];
                    i = 1;
                    if (char.IsSurrogate(c))
                    {
                        s = term.Substring(ipos, 2);
                        s = opos == 0 ? s : s.ToUpper(culture);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos] = opos++ == 0 ? c : char.ToUpper(c, culture);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i);
                        term.CopyTo(0, output, opos, term.Length);
                        opos += term.Length;                        
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("caseType");
            }
        }

        #endregion

        #region SplitPascalCasing

        /// <summary>
        /// Splits a Pascal-cased string into a phrase separated by a separator.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="separator">The separator, which defaults to a whitespace.</param>
        /// <returns>The splitted text.</returns>
        /// <remarks>Supports Utf8 and Ascii strings, not Unicode strings.</remarks>
        // NOTE does not support surrogates pairs at the moment
        public virtual string SplitPascalCasing(string text, char separator)
        {
            // be safe
            if (text == null)
                throw new ArgumentNullException("text");

            var input = text.ToCharArray();
            var output = new char[input.Length * 2];
            var opos = 0;
            var a = input.Length > 0 ? input[0] : char.MinValue;
            var upos = char.IsUpper(a) ? 1 : 0;

            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];
                if (char.IsUpper(c))
                {
                    output[opos++] = a;
                    if (upos == 0)
                    {
                        if (opos > 0)
                            output[opos++] = separator;
                        upos = i + 1;
                    }
                }
                else
                {
                    if (upos > 0)
                    {
                        if (upos < i && opos > 0)
                            output[opos++] = separator;
                        upos = 0;
                    }
                    output[opos++] = a;
                }
                a = c;
            }
            if (a != char.MinValue)
                output[opos++] = a;
            return new string(output, 0, opos);
        }

        #endregion

        #region ReplaceMany

        /// <summary>
        /// Returns a new string in which all occurences of specified strings are replaced by other specified strings.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The replacements definition.</param>
        /// <returns>The filtered string.</returns>
        public virtual string ReplaceMany(string text, IDictionary<string, string> replacements)
        {
            // be safe
            if (text == null)
                throw new ArgumentNullException("text");
            if (replacements == null)
                throw new ArgumentNullException("replacements");

            // Have done various tests, implementing my own "super fast" state machine to handle 
            // replacement of many items, or via regexes, but on short strings and not too
            // many replacements (which prob. is going to be our case) nothing can beat this...
            // (at least with safe and checked code -- we don't want unsafe/unchecked here)

            // Note that it will do chained-replacements ie replaced items can be replaced
            // in turn by another replacement (ie the order of replacements is important)

            return replacements.Aggregate(text, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }

        /// <summary>
        /// Returns a new string in which all occurences of specified characters are replaced by a specified character.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="chars">The characters to replace.</param>
        /// <param name="replacement">The replacement character.</param>
        /// <returns>The filtered string.</returns>
        public virtual string ReplaceMany(string text, char[] chars, char replacement)
        {
            // be safe
            if (text == null)
                throw new ArgumentNullException("text");
            if (chars == null)
                throw new ArgumentNullException("chars");

            // see note above

            return chars.Aggregate(text, (current, c) => current.Replace(c, replacement));
        }

        #endregion
    }
}
