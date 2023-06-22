// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Configuration options for the Marketplace.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigMarketplace)]
public class MarketplaceSettings
{
    /// <summary>
    /// Gets or sets the additional parameters that are sent to the Marketplace.
    /// </summary>
    public Dictionary<string, string> AdditionalParameters { get; set; } = new ();
}
