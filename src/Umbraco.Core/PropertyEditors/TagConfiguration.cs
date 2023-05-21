using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the tag value editor.
/// </summary>
public class TagConfiguration
{
    [ConfigurationField("group", "Tag group", "requiredfield", Description = "Define a tag group")]
    public string Group { get; set; } = "default";

    [ConfigurationField(
        "storageType",
        "Storage Type",
        "views/propertyeditors/tags/tags.prevalues.html",
        Description = "Select whether to store the tags in cache as JSON (default) or as CSV. The only benefits of storage as JSON is that you are able to have commas in a tag value")]
    public TagsStorageType StorageType { get; set; } = TagsStorageType.Json;

    // not a field
    public char Delimiter { get; set; }
}
