namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for a multiple textstring value editor.
/// </summary>
public class MultipleTextStringConfiguration
{
    /// <summary>
    /// Gets or sets the minimum number of text strings required.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of text strings allowed.
    /// </summary>
    public int Max { get; set; }
}
