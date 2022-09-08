using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a datatype model for editing.
/// </summary>
[DataContract(Name = "dataType", Namespace = "")]
public class DataTypeSave : EntityBasic
{
    /// <summary>
    ///     Gets or sets the action to perform.
    /// </summary>
    /// <remarks>
    ///     Some values (publish) are illegal here.
    /// </remarks>
    [DataMember(Name = "action", IsRequired = true)]
    [Required]
    public ContentSaveAction Action { get; set; }

    /// <summary>
    ///     Gets or sets the datatype editor.
    /// </summary>
    [DataMember(Name = "selectedEditor", IsRequired = true)]
    [Required]
    public string? EditorAlias { get; set; }

    /// <summary>
    ///     Gets or sets the datatype configuration fields.
    /// </summary>
    [DataMember(Name = "preValues")]
    public IEnumerable<DataTypeConfigurationFieldSave>? ConfigurationFields { get; set; }

    /// <summary>
    ///     Gets or sets the persisted data type.
    /// </summary>
    [IgnoreDataMember]
    public IDataType? PersistedDataType { get; set; }

    /// <summary>
    ///     Gets or sets the property editor.
    /// </summary>
    [IgnoreDataMember]
    public IDataEditor? PropertyEditor { get; set; }
}
