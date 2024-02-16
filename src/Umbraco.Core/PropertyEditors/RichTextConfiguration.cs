namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("blocks")]
    public RichTextBlockConfiguration[]? Blocks { get; set; } = null!;

    [ConfigurationField("mediaParentId")]
    public GuidUdi? MediaParentId { get; set; }

    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    public class RichTextBlockConfiguration : IBlockConfiguration
    {
        public Guid ContentElementTypeKey { get; set; }

        public Guid? SettingsElementTypeKey { get; set; }
    }
}
