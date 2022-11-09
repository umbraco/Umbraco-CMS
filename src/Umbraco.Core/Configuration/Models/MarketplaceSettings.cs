// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for marketplace.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigMarketplace)]
public class MarketplaceSettings
{
    /// <summary>
    ///     Gets or sets a value for the Active Directory domain.
    /// </summary>
    public Dictionary<string, string> AdditionalParameters { get; set; } = new ();
}
