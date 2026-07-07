// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for Umbraco Search.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigSearch)]
public class SearchSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether backoffice search and Delivery API querying should use the
    ///     legacy Examine based implementations instead of Umbraco Search.
    /// </summary>
    /// <remarks>
    ///     This is a temporary escape hatch. It is scheduled for removal along with the legacy Examine based
    ///     search implementations.
    /// </remarks>
    public bool UseLegacySearchServices { get; set; }
}
