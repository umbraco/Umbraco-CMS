using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "propertyType")]
public class PropertyTypeDisplay : PropertyTypeBasic
{
    [DataMember(Name = "editor")]
    [ReadOnly(true)]
    public string? Editor { get; set; }

    [DataMember(Name = "view")]
    [ReadOnly(true)]
    public string? View { get; set; }

    [DataMember(Name = "config")]
    [ReadOnly(true)]
    public IDictionary<string, object>? Config { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this property should be locked when editing.
    /// </summary>
    /// <remarks>
    ///     This is used for built in properties like the default MemberType
    ///     properties that should not be editable from the backoffice.
    /// </remarks>
    [DataMember(Name = "locked")]
    [ReadOnly(true)]
    public bool Locked { get; set; }

    /// <summary>
    ///     This is required for the UI editor to know if this particular property belongs to
    ///     an inherited item or the current item.
    /// </summary>
    [DataMember(Name = "contentTypeId")]
    [ReadOnly(true)]
    public int ContentTypeId { get; set; }

    /// <summary>
    ///     This is required for the UI editor to know which content type name this property belongs
    ///     to based on the property inheritance structure
    /// </summary>
    [DataMember(Name = "contentTypeName")]
    [ReadOnly(true)]
    public string? ContentTypeName { get; set; }
}
