using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "propertyType")]
public class PropertyTypeBasic
{
    /// <summary>
    ///     Gets a value indicating whether the property type is inherited through
    ///     content types composition.
    /// </summary>
    /// <remarks>
    ///     Inherited is true when the property is defined by a content type
    ///     higher in the composition, and not by the content type currently being
    ///     edited.
    /// </remarks>
    [DataMember(Name = "inherited")]
    public bool Inherited { get; set; }

    // needed - so we can handle alias renames
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"^([a-zA-Z]\w.*)$", ErrorMessage = "Invalid alias")]
    [DataMember(Name = "alias")]
    public string Alias { get; set; } = null!;

    [DataMember(Name = "description")]
    public string? Description { get; set; }

    [DataMember(Name = "validation")]
    public PropertyTypeValidation? Validation { get; set; }

    [DataMember(Name = "label")]
    [Required]
    public string Label { get; set; } = null!;

    [DataMember(Name = "sortOrder")]
    public int SortOrder { get; set; }

    [DataMember(Name = "dataTypeId")]
    [Required]
    public int DataTypeId { get; set; }

    [DataMember(Name = "dataTypeKey")]
    [ReadOnly(true)]
    public Guid DataTypeKey { get; set; }

    [DataMember(Name = "dataTypeName")]
    [ReadOnly(true)]
    public string? DataTypeName { get; set; }

    [DataMember(Name = "dataTypeIcon")]
    [ReadOnly(true)]
    public string? DataTypeIcon { get; set; }

    // SD: Is this really needed ?
    [DataMember(Name = "groupId")]
    public int GroupId { get; set; }

    [DataMember(Name = "allowCultureVariant")]
    public bool AllowCultureVariant { get; set; }

    [DataMember(Name = "allowSegmentVariant")]
    public bool AllowSegmentVariant { get; set; }

    [DataMember(Name = "labelOnTop")]
    public bool LabelOnTop { get; set; }
}
