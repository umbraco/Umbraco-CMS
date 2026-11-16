namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration shared by the media picker value editors.
/// </summary>
public abstract class MediaPickerConfigurationBase : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the media type filter.
    /// </summary>
    [ConfigurationField("filter")]
    public string? Filter { get; set; }

    /// <summary>
    /// Gets or sets the start node ID for the media picker.
    /// </summary>
    [ConfigurationField("startNodeId")]
    public Guid? StartNodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether local focal point editing is enabled.
    /// </summary>
    [ConfigurationField("enableLocalFocalPoint")]
    public bool EnableLocalFocalPoint { get; set; }

    /// <summary>
    /// Gets or sets the configured image crops.
    /// </summary>
    [ConfigurationField("crops")]
    public CropConfiguration[]? Crops { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    /// <summary>
    /// Represents an image crop configuration.
    /// </summary>
    public class CropConfiguration
    {
        /// <summary>
        /// Gets or sets the alias of the crop.
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the width of the crop in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the crop in pixels.
        /// </summary>
        public int Height { get; set; }
    }
}
