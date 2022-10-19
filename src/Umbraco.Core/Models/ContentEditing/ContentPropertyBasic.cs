using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a content property to be saved
/// </summary>
[DataContract(Name = "property", Namespace = "")]
public class ContentPropertyBasic
{
    /// <summary>
    ///     This is the PropertyData ID
    /// </summary>
    /// <remarks>
    ///     This is not really used for anything
    /// </remarks>
    [DataMember(Name = "id", IsRequired = true)]
    [Required]
    public int Id { get; set; }

    [DataMember(Name = "dataTypeKey", IsRequired = false)]
    [ReadOnly(true)]
    public Guid DataTypeKey { get; set; }

    [DataMember(Name = "value")]
    public object? Value { get; set; }

    [DataMember(Name = "alias", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string Alias { get; set; } = null!;

    [DataMember(Name = "editor", IsRequired = false)]
    public string? Editor { get; set; }

    /// <summary>
    ///     Flags the property to denote that it can contain sensitive data
    /// </summary>
    [DataMember(Name = "isSensitive", IsRequired = false)]
    public bool IsSensitive { get; set; }

    /// <summary>
    ///     The culture of the property
    /// </summary>
    /// <remarks>
    ///     If this is a variant property then this culture value will be the same as it's variant culture but if this
    ///     is an invariant property then this will be a null value.
    /// </remarks>
    [DataMember(Name = "culture")]
    [ReadOnly(true)]
    public string? Culture { get; set; }

    /// <summary>
    ///     The segment of the property
    /// </summary>
    /// <remarks>
    ///     The segment value of a property can always be null but can only have a non-null value
    ///     when the property can be varied by segment.
    /// </remarks>
    [DataMember(Name = "segment")]
    [ReadOnly(true)]
    public string? Segment { get; set; }

    /// <summary>
    ///     Used internally during model mapping
    /// </summary>
    [IgnoreDataMember]
    public IDataEditor? PropertyEditor { get; set; }

    /// <summary>
    ///     Used internally during model mapping
    /// </summary>
    [DataMember(Name = "supportsReadOnly")]
    [ReadOnly(true)]
    public bool SupportsReadOnly { get; set; }
}
