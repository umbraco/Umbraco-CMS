using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the tag value editor.
/// </summary>
public class TagConfiguration
{
    /// <summary>
    /// Gets or sets the tag group name.
    /// </summary>
    [ConfigurationField("group")]
    public string Group { get; set; } = "default";

    /// <summary>
    /// Gets or sets the storage type for tags (CSV or JSON).
    /// </summary>
    [ConfigurationField("storageType")]
    public TagsStorageType StorageType { get; set; } = TagsStorageType.Json;

    /// <summary>
    /// Gets or sets the delimiter character used for CSV storage.
    /// </summary>
    public char Delimiter { get; set; }
}
