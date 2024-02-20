using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a datatype configuration field model for editing.
/// </summary>
[DataContract(Name = "preValue", Namespace = "")]
public class DataTypeConfigurationFieldDisplay : DataTypeConfigurationFieldSave
{
    /// <summary>
    ///     This allows for custom configuration to be injected into the pre-value editor
    /// </summary>
    [DataMember(Name = "config")]
    public IDictionary<string, object>? Config { get; set; }
}
