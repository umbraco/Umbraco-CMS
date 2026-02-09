namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the block configurations for the rich text editor.
    /// </summary>
    [ConfigurationField("blocks")]
    public RichTextBlockConfiguration[]? Blocks { get; set; } = Array.Empty<RichTextBlockConfiguration>();

    /// <summary>
    /// Gets or sets the media parent ID for uploaded media.
    /// </summary>
    [ConfigurationField("mediaParentId")]
    public Guid? MediaParentId { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    /// <summary>
    /// Represents a block configuration for the rich text editor.
    /// </summary>
    public class RichTextBlockConfiguration : IBlockConfiguration
    {
        /// <summary>
        /// Gets or sets the unique key of the content element type for this block.
        /// </summary>
        public Guid ContentElementTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the unique key of the settings element type for this block, if any.
        /// </summary>
        public Guid? SettingsElementTypeKey { get; set; }
    }
}
