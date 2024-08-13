namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the media picker value editor.
/// </summary>
public class MediaPicker3Configuration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("filter")]
    public string? Filter { get; set; }

    [ConfigurationField("multiple")]
    public bool Multiple { get; set; }

    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("startNodeId")]
    public Guid? StartNodeId { get; set; }

    [ConfigurationField("enableLocalFocalPoint")]
    public bool EnableLocalFocalPoint { get; set; }

    [ConfigurationField("crops")]
    public CropConfiguration[]? Crops { get; set; }

    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    public class NumberRange
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }

    public class CropConfiguration
    {
        public string? Alias { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
