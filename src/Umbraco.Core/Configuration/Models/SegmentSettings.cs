using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for segment settings.
/// </summary>
public class SegmentSettings
{
    private const bool StaticEnabled = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the usage of segments is enabled.
    /// </summary>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;
}
