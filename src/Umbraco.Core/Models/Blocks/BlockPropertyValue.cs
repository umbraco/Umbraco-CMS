using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Blocks;

public sealed class BlockPropertyValue : ValueModelBase
{
    // Used during deserialization to populate the property value/property type of a block item content property
    [JsonIgnore]
    public IPropertyType? PropertyType { get; set; }

    public string? EditorAlias => PropertyType?.PropertyEditorAlias;
}
