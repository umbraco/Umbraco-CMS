namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the media picker value editor.
/// </summary>
public class MediaPicker3Configuration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the media type filter.
    /// </summary>
    [ConfigurationField("filter")]
    public string? Filter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple media items can be selected.
    /// </summary>
    [ConfigurationField("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets the validation limits for the number of selected items.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

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

    /// <summary>
    /// Gets or sets the alt text mode for media items.
    /// </summary>
    /// <remarks>
    /// Valid values: "off", "altText", "decorative". Defaults to "off" so that pre-existing data types
    /// whose stored configuration does not contain this field are not silently upgraded to show alt text
    /// prompts. The TypeScript property editor UI also falls back to "off" when the field is absent,
    /// ensuring both tiers agree.
    /// </remarks>
    [ConfigurationField("altTextMode")]
    public string AltTextMode { get; set; } = "off";

    /// <summary>
    /// Gets or sets a value indicating whether to hide the zoom control in the crop editor.
    /// </summary>
    [ConfigurationField("hideZoomCrop")]
    public bool HideZoomCrop { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether per-crop alternative text is enabled.
    /// </summary>
    [ConfigurationField("enableAltTextPerCrop")]
    public bool EnableAltTextPerCrop { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    /// <summary>
    /// Represents a numeric range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        /// Gets or sets the minimum value of the range.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the range.
        /// </summary>
        public int? Max { get; set; }
    }

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
