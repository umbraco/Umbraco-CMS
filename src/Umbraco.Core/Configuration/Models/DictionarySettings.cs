// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for dictionary settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigDictionary)]
public class DictionarySettings
{
    private const bool StaticEnableValueSearch = false;

    /// <summary>
    ///     Gets or sets a value indicating whether to enable searching in dictionary values in addition to keys.
    /// </summary>
    /// <remarks>
    ///     When enabled, the GetDictionaryItemDescendants method will search both dictionary keys and translation values.
    ///     This may impact performance when dealing with large numbers of dictionary items.
    /// </remarks>
    [DefaultValue(StaticEnableValueSearch)]
    public bool EnableValueSearch { get; set; } = StaticEnableValueSearch;
}
