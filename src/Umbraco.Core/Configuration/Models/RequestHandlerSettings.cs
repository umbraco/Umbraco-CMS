// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for request handler settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigRequestHandler)]
    public class RequestHandlerSettings
    {
        internal const bool StaticAddTrailingSlash = true;
        internal const string StaticConvertUrlsToAscii = "try";

        internal static readonly CharItem[] DefaultCharCollection =
        {
            new CharItem { Char = " ", Replacement = "-" },
            new CharItem { Char = "\"", Replacement = string.Empty },
            new CharItem { Char = "'", Replacement = string.Empty },
            new CharItem { Char = "%", Replacement = string.Empty },
            new CharItem { Char = ".", Replacement = string.Empty },
            new CharItem { Char = ";", Replacement = string.Empty },
            new CharItem { Char = "/", Replacement = string.Empty },
            new CharItem { Char = "\\", Replacement = string.Empty },
            new CharItem { Char = ":", Replacement = string.Empty },
            new CharItem { Char = "#", Replacement = string.Empty },
            new CharItem { Char = "+", Replacement = "plus" },
            new CharItem { Char = "*", Replacement = "star" },
            new CharItem { Char = "&", Replacement = string.Empty },
            new CharItem { Char = "?", Replacement = string.Empty },
            new CharItem { Char = "æ", Replacement = "ae" },
            new CharItem { Char = "ä", Replacement = "ae" },
            new CharItem { Char = "ø", Replacement = "oe" },
            new CharItem { Char = "ö", Replacement = "oe" },
            new CharItem { Char = "å", Replacement = "aa" },
            new CharItem { Char = "ü", Replacement = "ue" },
            new CharItem { Char = "ß", Replacement = "ss" },
            new CharItem { Char = "|", Replacement = "-" },
            new CharItem { Char = "<", Replacement = string.Empty },
            new CharItem { Char = ">", Replacement = string.Empty }
        };

        /// <summary>
        /// Gets or sets a value indicating whether to add a trailing slash to URLs.
        /// </summary>
        [DefaultValue(StaticAddTrailingSlash)]
        public bool AddTrailingSlash { get; set; } = StaticAddTrailingSlash;

        /// <summary>
        /// Gets or sets a value indicating whether to convert URLs to ASCII (valid values: "true", "try" or "false").
        /// </summary>
        [DefaultValue(StaticConvertUrlsToAscii)]
        public string ConvertUrlsToAscii { get; set; } = StaticConvertUrlsToAscii;

        /// <summary>
        /// Gets a value indicating whether URLs should be converted to ASCII.
        /// </summary>
        public bool ShouldConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("true");

        /// <summary>
        /// Gets a value indicating whether URLs should be tried to be converted to ASCII.
        /// </summary>
        public bool ShouldTryConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("try");

        // We need to special handle ":", as this character is special in keys

        // TODO: implement from configuration

        //// var collection = _configuration.GetSection(Prefix + "CharCollection").GetChildren()
        ////    .Select(x => new CharItem()
        ////    {
        ////        Char = x.GetValue<string>("Char"),
        ////        Replacement = x.GetValue<string>("Replacement"),
        ////    }).ToArray();

        //// if (collection.Any() || _configuration.GetSection("Prefix").GetChildren().Any(x =>
        ////    x.Key.Equals("CharCollection", StringComparison.OrdinalIgnoreCase)))
        //// {
        ////    return collection;
        //// }

        //// return DefaultCharCollection;

        private IEnumerable<IChar> _charCollection;

        /// <summary>
        /// Gets or sets a value indicating whether to use empty list of characters.
        /// UrlReplaceCharacters is empty during initialization (first time only), it's a trick for unit testing.
        /// </summary>
        public bool UseEmpty { get; set; }
        /// <summary>
        /// Gets or sets a value for the default character collection for replacements.
        /// </summary>
        /// WB-TODO
        public IEnumerable<IChar> CharCollection
        {
            get
            {
                if (UseEmpty)
                {
                    return Enumerable.Empty<IChar>();
                }

                return SetCharCollection();
            }
        }

        /// <summary>
        /// Gets or sets list of characters that can be overwritten from configuration.
        /// </summary>
        public IEnumerable<CharItem> UrlReplaceCharacters { get; set; }

        /// <summary>
        /// Returns a combination of default characters and from configuration.

        /// </summary>
        /// <returns></returns>
        internal IEnumerable<IChar> SetCharCollection()
        {
            if (UrlReplaceCharacters?.Any() != true)
            {
                return DefaultCharCollection;
            }

            var charCollection = new List<IChar>();
            foreach (var defaultChar in DefaultCharCollection)
            {
                if (UrlReplaceCharacters.Any(x => x.Char == defaultChar.Char))
                    continue;

                charCollection.Add(defaultChar);
            }

            return charCollection;
        }

        /// <summary>
        /// Defines a character replacement.
        /// </summary>
        public class CharItem : IChar
        {
            /// <inheritdoc/>
            public string Char { get; set; }

            /// <inheritdoc/>
            public string Replacement { get; set; }
        }
    }
}
