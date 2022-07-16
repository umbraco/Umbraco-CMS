using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings
{
    /// <summary>
    /// New default implementation of string functions for short strings such as aliases or URL segments.
    /// </summary>
    /// <remarks>
    /// <para>Not optimized to work on large bodies of text.</para>
    /// <para>Meant to replace <c>LegacyShortStringHelper</c> where/when backward compatibility is not an issue.</para>
    /// <para>NOTE: pre-filters run _before_ the string is re-encoded.</para>
    /// </remarks>
    public class DefaultShortStringHelper : IShortStringHelper
    {
        #region Ctor, consts and vars

        public DefaultShortStringHelper(IOptions<RequestHandlerSettings> settings)
        {
            _config = new DefaultShortStringHelperConfig().WithDefault(settings.Value);
        }

        // clones the config so it cannot be changed at runtime
        public DefaultShortStringHelper(DefaultShortStringHelperConfig config)
        {
            _config = config.Clone();
        }

        // see notes for CleanAsciiString
        //// beware! the order is quite important here!
        //const string ValidStringCharactersSource = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //readonly static char[] ValidStringCharacters;

        private readonly DefaultShortStringHelperConfig _config;

        // see notes for CleanAsciiString
        //static DefaultShortStringHelper()
        //{
        //    ValidStringCharacters = ValidStringCharactersSource.ToCharArray();
        //}

        #endregion

        #region Filters

        // ok to be static here because it's not configurable in any way
        private static readonly char[] InvalidFileNameChars =
            Path.GetInvalidFileNameChars()
            .Union("!*'();:@&=+$,/?%#[]-~{}\"<>\\^`| ".ToCharArray())
            .Distinct()
            .ToArray();

        public static bool IsValidFileNameChar(char c)
        {
            return InvalidFileNameChars.Contains(c) == false;
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
            return CleanStringForSafeAlias(text, _config.DefaultCulture);
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
        public virtual string CleanStringForSafeAlias(string text, string culture)
        {
            return CleanString(text, CleanStringType.Alias, culture);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used in an URL segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe URL segment.</returns>
        /// <remarks>
        /// <para>The string will be cleaned in the context of the default culture.</para>
        /// <para>Url segments are Ascii only (no accents...).</para>
        /// </remarks>
        public virtual string CleanStringForUrlSegment(string text)
        {
            return CleanStringForUrlSegment(text, _config.DefaultCulture);
        }

        /// <summary>
        /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an URL segment.
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe URL segment.</returns>
        /// <remarks>
        /// <para>Url segments are Ascii only (no accents...).</para>
        /// </remarks>
        public virtual string CleanStringForUrlSegment(string text, string? culture)
        {
            return CleanString(text, CleanStringType.UrlSegment, culture);
        }

        /// <summary>
        /// Cleans a string, in the context of the default culture, to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a URL).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <returns>The safe filename.</returns>
        /// <remarks>Legacy says this was used to "overcome an issue when Umbraco is used in IE in an intranet environment" but that issue is not documented.</remarks>
        public virtual string CleanStringForSafeFileName(string text)
        {
            return CleanStringForSafeFileName(text, _config.DefaultCulture);
        }

        /// <summary>
        /// Cleans a string to produce a string that can safely be used as a filename,
        /// both internally (on disk) and externally (as a URL).
        /// </summary>
        /// <param name="text">The text to filter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The safe filename.</returns>
        public virtual string CleanStringForSafeFileName(string text, string culture)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            culture = culture ?? string.Empty;
            text = text.ReplaceMany(Path.GetInvalidFileNameChars(), '-');

            var name = Path.GetFileNameWithoutExtension(text);
            var ext = Path.GetExtension(text); // includes the dot, empty if no extension

            Debug.Assert(name != null, "name != null");
            if (name.Length > 0)
            {
                name = CleanString(name, CleanStringType.FileName, culture);
            }

            Debug.Assert(ext != null, "ext != null");
            if (ext.Length > 0)
            {
                ext = CleanString(ext.Substring(1), CleanStringType.FileName, culture);
            }

            return ext.Length > 0 ? name + "." + ext : name;
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

        private const byte StateBreak = 1;
        private const byte StateUp = 2;
        private const byte StateWord = 3;
        private const byte StateAcronym = 4;

        /// <summary>
        /// Cleans a string.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the default culture.</remarks>
        public string CleanString(string text, CleanStringType stringType) => CleanString(text, stringType, _config.DefaultCulture, null);

        /// <summary>
        /// Cleans a string, using a specified separator.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The clean string.</returns>
        /// <remarks>The string is cleaned in the context of the default culture.</remarks>
        public string CleanString(string text, CleanStringType stringType, char separator) => CleanString(text, stringType, _config.DefaultCulture, separator);

        /// <summary>
        /// Cleans a string in the context of a specified culture.
        /// </summary>
        /// <param name="text">The text to clean.</param>
        /// <param name="stringType">A flag indicating the target casing and encoding of the string. By default,
        /// strings are cleaned up to camelCase and Ascii.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The clean string.</returns>
        public string CleanString(string text, CleanStringType stringType, string? culture)
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
        public string CleanString(string text, CleanStringType stringType, char separator, string culture)
        {
            return CleanString(text, stringType, culture, separator);
        }

        protected virtual string CleanString(string text, CleanStringType stringType, string? culture, char? separator)
        {
            // be safe
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            culture = culture ?? string.Empty;

            // get config
            DefaultShortStringHelperConfig.Config config = _config.For(stringType, culture);
            stringType = config.StringTypeExtend(stringType);

            // apply defaults
            if ((stringType & CleanStringType.CaseMask) == CleanStringType.None)
            {
                stringType |= CleanStringType.CamelCase;
            }

            if ((stringType & CleanStringType.CodeMask) == CleanStringType.None)
            {
                stringType |= CleanStringType.Ascii;
            }

            // use configured unless specified
            separator = separator ?? config.Separator;

            // apply pre-filter
            if (config.PreFilter != null)
            {
                text = config.PreFilter(text);
            }

            // apply replacements
            //if (config.Replacements != null)
            //    text = ReplaceMany(text, config.Replacements);

            // recode
            CleanStringType codeType = stringType & CleanStringType.CodeMask;
            switch (codeType)
            {
                case CleanStringType.Ascii:
                    text = Utf8ToAsciiConverter.ToAsciiString(text);
                    break;
                case CleanStringType.TryAscii:
                    const char ESC = (char) 27;
                    var ctext = Utf8ToAsciiConverter.ToAsciiString(text, ESC);
                    if (ctext.Contains(ESC) == false)
                    {
                        text = ctext;
                    }

                    break;
                default:
                    text = RemoveSurrogatePairs(text);
                    break;
            }

            // clean
            text = CleanCodeString(text, stringType, separator.Value, culture, config);

            // apply post-filter
            if (config.PostFilter != null)
            {
                text = config.PostFilter(text);
            }

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
        internal string CleanCodeString(string text, CleanStringType caseType, char separator, string culture, DefaultShortStringHelperConfig.Config config)
        {
            int opos = 0, ipos = 0;
            var state = StateBreak;

            culture = culture ?? string.Empty;
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

                // what should I do with surrogates? - E.g emojis like 🎈
                // no idea, really, so they are not supported at the moment and we just continue
                var isPair = char.IsSurrogate(c);
                if (isPair)
                {
                    continue;
                }


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
                            {
                                output[opos++] = separator;
                            }

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
                            {
                                output[opos++] = separator;
                            }
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
                            {
                                i -= 1; // handle that char again, in another state - not part of the acronym
                            }

                            if (i - ipos > 1) // single-char can't be an acronym
                            {
                                CopyTerm(input, ipos, output, ref opos, i - ipos, caseType, culture, true);
                                ipos = i;
                                state = isTerm ? StateWord : StateBreak;
                                if (state != StateBreak && separator != char.MinValue)
                                {
                                    output[opos++] = separator;
                                }
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
        internal void CopyTerm(string input, int ipos, char[] output, ref int opos, int len, CleanStringType caseType, string culture, bool isAcronym)
        {
            var term = input.Substring(ipos, len);
            CultureInfo cultureInfo = string.IsNullOrEmpty(culture) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);

            if (isAcronym)
            {
                if ((caseType == CleanStringType.CamelCase && len <= 2 && opos > 0) ||
                        (caseType == CleanStringType.PascalCase && len <= 2) ||
                        caseType == CleanStringType.UmbracoCase)
                {
                    caseType = CleanStringType.Unchanged;
                }
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
                    term = term.ToLower(cultureInfo);
                    term.CopyTo(0, output, opos, term.Length);
                    opos += term.Length;
                    break;

                case CleanStringType.UpperCase:
                    term = term.ToUpper(cultureInfo);
                    term.CopyTo(0, output, opos, term.Length);
                    opos += term.Length;
                    break;

                case CleanStringType.CamelCase:
                    c = term[0];
                    i = 1;
                    if (char.IsSurrogate(c))
                    {
                        s = term.Substring(ipos, 2);
                        s = opos == 0 ? s.ToLower(cultureInfo) : s.ToUpper(cultureInfo);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos] = opos++ == 0 ? char.ToLower(c, cultureInfo) : char.ToUpper(c, cultureInfo);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i).ToLower(cultureInfo);
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
                        s = s.ToUpper(cultureInfo);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos++] = char.ToUpper(c, cultureInfo);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i).ToLower(cultureInfo);
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
                        s = opos == 0 ? s : s.ToUpper(cultureInfo);
                        s.CopyTo(0, output, opos, s.Length);
                        opos += s.Length;
                        i++; // surrogate pair len is 2
                    }
                    else
                    {
                        output[opos] = opos++ == 0 ? c : char.ToUpper(c, cultureInfo);
                    }
                    if (len > i)
                    {
                        term = term.Substring(i);
                        term.CopyTo(0, output, opos, term.Length);
                        opos += term.Length;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(caseType));
            }
        }

        #endregion

        #region SplitPascalCasing

        /// <summary>
        /// Splits a Pascal-cased string into a phrase separated by a separator.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="separator">The separator, which defaults to a whitespace.</param>
        /// <returns>The split text.</returns>
        /// <remarks>Supports Utf8 and Ascii strings, not Unicode strings.</remarks>
        // NOTE does not support surrogates pairs at the moment
        public virtual string SplitPascalCasing(string text, char separator)
        {
            // be safe
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

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
                        {
                            output[opos++] = separator;
                        }

                        upos = i + 1;
                    }
                }
                else
                {
                    if (upos > 0)
                    {
                        if (upos < i && opos > 0)
                        {
                            output[opos++] = separator;
                        }

                        upos = 0;
                    }

                    output[opos++] = a;
                }

                a = c;
            }

            if (a != char.MinValue)
            {
                output[opos++] = a;
            }

            return new string(output, 0, opos);
        }

        #endregion
    }
}
