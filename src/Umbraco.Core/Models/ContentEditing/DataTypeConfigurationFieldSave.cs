using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a datatype configuration field model for editing.
/// </summary>
[DataContract(Name = "preValue", Namespace = "")]
public class DataTypeConfigurationFieldSave
{
    /// <summary>
    ///     Gets or sets the configuration field key.
    /// </summary>
    [DataMember(Name = "key", IsRequired = true)]
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the configuration field value.
    /// </summary>
    [DataMember(Name = "value", IsRequired = true)]
    public object? Value { get; set; }
}
