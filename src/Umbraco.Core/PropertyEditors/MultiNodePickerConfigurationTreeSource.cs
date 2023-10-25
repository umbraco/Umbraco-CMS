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

    [DataMember(Name = "dynamicRoot")]
    public MultiNodePickerConfigurationQueryFilter? StartNodeQueryFilter { get; set; }

    [DataMember(Name = "id")]
    public Udi? StartNodeId { get; set; }
}

[DataContract]
public class MultiNodePickerConfigurationQueryFilter
{
    [DataMember(Name = "originAlias")]
    public string OriginAlias { get; set; } = string.Empty;

    [DataMember(Name = "originKey")]
    public Guid? OriginKey { get; set; }

    [DataMember(Name = "querySteps")]
    public MultiNodePickerConfigurationQueryFilterStep[] Filter { get; set; } = Array.Empty<MultiNodePickerConfigurationQueryFilterStep>();
}

[DataContract]
public class MultiNodePickerConfigurationQueryFilterStep
{
    [DataMember(Name = "alias")]
    public string DirectionAlias { get; set; } = string.Empty;

    [DataMember(Name = "anyOfDocTypeAlias")]
    public IEnumerable<string> AnyOfDocTypeAlias { get; set; } = Array.Empty<string>();
}

