// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Determines how a dictionary item filter is matched against the dictionary item key.
/// </summary>
public enum DictionaryKeySearchMode
{
    /// <summary>
    ///     Match keys that start with the filter.
    /// </summary>
    StartsWith,

    /// <summary>
    ///     Match keys that contain the filter at any position. This finds more items than <see cref="StartsWith" />,
    ///     at the cost of a less efficient lookup on installations with a large number of dictionary items.
    /// </summary>
    Contains,
}
