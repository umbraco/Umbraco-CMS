using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Defines an available property editor to be able to select for a data type
/// </summary>
[DataContract(Name = "propertyEditor", Namespace = "")]
public class PropertyEditorBasic
{
    [DataMember(Name = "alias")]
    public string? Alias { get; set; }
}
