// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for external member settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigExternalMembers)]
public class ExternalMemberSettings
{
    /// <summary>
    ///     The default value for whether external-only members are enabled.
    /// </summary>
    internal const bool StaticEnabled = false;

    /// <summary>
    ///     Gets or sets a value indicating whether external-only members are enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;
}
