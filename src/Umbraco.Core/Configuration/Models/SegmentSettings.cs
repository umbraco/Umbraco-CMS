using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for segment settings.
/// </summary>
public class SegmentSettings
{
    private const bool StaticEnabled = false;
    private const bool StaticAllowCreation = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the usage of segments is enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    ///     Gets or sets a value indicating whether the creation of non-existing segments is allowed.
    /// </summary>
    public bool AllowCreation { get; set; } = StaticAllowCreation;
}
