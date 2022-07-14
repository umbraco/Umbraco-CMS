// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for active directory settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigActiveDirectory)]
[Obsolete("This is not used anymore. Will be removed in Umbraco 12")]public class ActiveDirectorySettings
{
    /// <summary>
    ///     Gets or sets a value for the Active Directory domain.
    /// </summary>
    public string? Domain { get; set; }
}
