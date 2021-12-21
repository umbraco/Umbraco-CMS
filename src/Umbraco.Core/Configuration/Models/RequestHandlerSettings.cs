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
        internal const bool StaticEnableDefaultCharReplacements = true;

        internal static readonly CharItem[] DefaultCharCollection =
        {
            new () { Char = " ", Replacement = "-" },
            new () { Char = "\"", Replacement = string.Empty },
            new () { Char = "'", Replacement = string.Empty },
            new () { Char = "%", Replacement = string.Empty },
            new () { Char = ".", Replacement = string.Empty },
            new () { Char = ";", Replacement = string.Empty },
            new () { Char = "/", Replacement = string.Empty },
            new () { Char = "\\", Replacement = string.Empty },
            new () { Char = ":", Replacement = string.Empty },
            new () { Char = "#", Replacement = string.Empty },
            new () { Char = "+", Replacement = "plus" },
            new () { Char = "*", Replacement = "star" },
            new () { Char = "&", Replacement = string.Empty },
            new () { Char = "?", Replacement = string.Empty },
            new () { Char = "æ", Replacement = "ae" },
            new () { Char = "ä", Replacement = "ae" },
            new () { Char = "ø", Replacement = "oe" },
            new () { Char = "ö", Replacement = "oe" },
            new () { Char = "å", Replacement = "aa" },
            new () { Char = "ü", Replacement = "ue" },
            new () { Char = "ß", Replacement = "ss" },
            new () { Char = "|", Replacement = "-" },
            new () { Char = "<", Replacement = string.Empty },
            new () { Char = ">", Replacement = string.Empty }
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

        /// <summary>
        /// Disable all default character replacements
        /// </summary>
        [DefaultValue(StaticEnableDefaultCharReplacements)]
        public bool EnableDefaultCharReplacements { get; set; } = StaticEnableDefaultCharReplacements;

        /// <summary>
        /// Add additional character replacements, or override defaults
        /// </summary>
        public IEnumerable<CharItem> CharCollection { get; set; }

        /// <summary>
        /// Get concatenated user and default character replacements
        /// taking into account <see cref="EnableDefaultCharReplacements"/>
        /// </summary>
        public IEnumerable<CharItem> GetCharReplacements()
        {
            // TODO We need to special handle ":", as this character is special in keys

            if (!EnableDefaultCharReplacements)
            {
                return CharCollection;
            }

            if (CharCollection == null || !CharCollection.Any())
            {
                return DefaultCharCollection;
            }

            foreach (var defaultReplacement in DefaultCharCollection)
            {
                foreach (var userReplacement in CharCollection)
                {
                    if (userReplacement.Char == defaultReplacement.Char)
                    {
                        defaultReplacement.Replacement = userReplacement.Replacement;
                    }
                }
            }

            var mergedCollections = DefaultCharCollection.Union<CharItem>(CharCollection, new CharacterReplacementEqualityComparer());

            return mergedCollections;
        }
    }
}
