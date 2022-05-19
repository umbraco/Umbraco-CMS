using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the 'startNode' value for the <see cref="MultiNodePickerConfiguration" />
/// </summary>
[DataContract]
public class MultiNodePickerConfigurationTreeSource
{
    [DataMember(Name = "type")]
    public string? ObjectType { get; set; }

    [DataMember(Name = "query")]
    public string? StartNodeQuery { get; set; }

    [DataMember(Name = "id")]
    public Udi? StartNodeId { get; set; }
}
