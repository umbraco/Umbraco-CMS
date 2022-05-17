using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the media picker value editor.
/// </summary>
public class MediaPicker3Configuration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("filter", "Accepted types", "treesourcetypepicker", Description = "Limit to specific types")]
    public string? Filter { get; set; }

    [ConfigurationField("multiple", "Pick multiple items", "boolean", Description = "Outputs a IEnumerable")]
    public bool Multiple { get; set; }

    [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of medias")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("startNodeId", "Start node", "mediapicker")]
    public Udi? StartNodeId { get; set; }

    [ConfigurationField("enableLocalFocalPoint", "Enable Focal Point", "boolean")]
    public bool EnableLocalFocalPoint { get; set; }

    [ConfigurationField(
        "crops",
        "Image Crops",
        "views/propertyeditors/mediapicker3/prevalue/mediapicker3.crops.html",
        Description = "Local crops, stored on document")]
    public CropConfiguration[]? Crops { get; set; }

    [ConfigurationField(
        Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
        "Ignore User Start Nodes",
        "boolean",
        Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
    public bool IgnoreUserStartNodes { get; set; }

    [DataContract]
    public class NumberRange
    {
        [DataMember(Name = "min")]
        public int? Min { get; set; }

        [DataMember(Name = "max")]
        public int? Max { get; set; }
    }

    [DataContract]
    public class CropConfiguration
    {
        [DataMember(Name = "alias")]
        public string? Alias { get; set; }

        [DataMember(Name = "label")]
        public string? Label { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }
    }
}
