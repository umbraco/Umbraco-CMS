using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the tag value editor.
/// </summary>
public class TagConfiguration
{
    [ConfigurationField("group")]
    public string Group { get; set; } = "default";

    [ConfigurationField("storageType")]
    public TagsStorageType StorageType { get; set; } = TagsStorageType.Json;

    // not a field
    public char Delimiter { get; set; }
}
