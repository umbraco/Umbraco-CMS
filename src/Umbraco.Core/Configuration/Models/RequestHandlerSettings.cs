// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for request handler settings.
    /// </summary>
    public class RequestHandlerSettings
    {
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
        public bool AddTrailingSlash { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to convert URLs to ASCII (valid values: "true", "try" or "false").
        /// </summary>
        public string ConvertUrlsToAscii { get; set; } = "try";

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

        /// <summary>
        /// Gets or sets a value for the default character collection for replacements.
        /// </summary>
        public IEnumerable<IChar> CharCollection { get; set; } = DefaultCharCollection;

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
