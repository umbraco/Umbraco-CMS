// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for request handler settings.
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
            new () { Char = ">", Replacement = string.Empty },
    };

    /// <summary>
    ///     Gets or sets a value indicating whether to add a trailing slash to URLs.
    /// </summary>
    [DefaultValue(StaticAddTrailingSlash)]
    public bool AddTrailingSlash { get; set; } = StaticAddTrailingSlash;

    /// <summary>
    ///     Gets or sets a value indicating whether to convert URLs to ASCII (valid values: "true", "try" or "false").
    /// </summary>
    [DefaultValue(StaticConvertUrlsToAscii)]
    public string ConvertUrlsToAscii { get; set; } = StaticConvertUrlsToAscii;

    /// <summary>
    ///     Gets a value indicating whether URLs should be converted to ASCII.
    /// </summary>
    public bool ShouldConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("true");

    /// <summary>
    ///     Gets a value indicating whether URLs should be tried to be converted to ASCII.
    /// </summary>
    public bool ShouldTryConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("try");

    /// <summary>
    ///     Disable all default character replacements
    /// </summary>
    [DefaultValue(StaticEnableDefaultCharReplacements)]
    public bool EnableDefaultCharReplacements { get; set; } = StaticEnableDefaultCharReplacements;

    /// <summary>
    ///     Add additional character replacements, or override defaults
    /// </summary>
    [Obsolete(
        "Use the GetCharReplacements extension method in the Umbraco.Extensions namespace instead. Scheduled for removal in V11")]
    public IEnumerable<IChar> CharCollection { get; set; } = DefaultCharCollection;

    /// <summary>
    ///     Add additional character replacements, or override defaults
    /// </summary>
    public IEnumerable<CharItem>? UserDefinedCharCollection { get; set; }
}
