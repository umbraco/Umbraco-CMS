using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content property that is displayed in the UI
/// </summary>
[DataContract(Name = "property", Namespace = "")]
public class ContentPropertyDisplay : ContentPropertyBasic
{
    public ContentPropertyDisplay()
    {
        Config = new Dictionary<string, object>();
        Validation = new PropertyTypeValidation();
    }

    [DataMember(Name = "variations")]
    public ContentVariation Variations { get; set; }

    [DataMember(Name = "label", IsRequired = true)]
    [Required]
    public string? Label { get; set; }

    [DataMember(Name = "description")]
    public string? Description { get; set; }

    [DataMember(Name = "view", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string? View { get; set; }

    [Obsolete("The value type parameter of the dictionary will be made nullable in V11, use ConfigNullable instead.")]
    [DataMember(Name = "config")]
    public IDictionary<string, object>? Config { get; set; }

    // TODO: Obsolete in V11.
    [IgnoreDataMember]
    public IDictionary<string, object?>? ConfigNullable { get => Config!; set => Config = value!; }

    [DataMember(Name = "hideLabel")]
    public bool HideLabel { get; set; }

    [DataMember(Name = "labelOnTop")]
    public bool? LabelOnTop { get; set; }

    [DataMember(Name = "validation")]
    public PropertyTypeValidation Validation { get; set; }

    [DataMember(Name = "readonly")]
    public bool Readonly { get; set; }
}
