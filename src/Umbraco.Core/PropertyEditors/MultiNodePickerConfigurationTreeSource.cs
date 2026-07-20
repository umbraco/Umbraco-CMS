using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the 'startNode' value for the <see cref="MultiNodePickerConfiguration" />
/// </summary>
[DataContract]
public class MultiNodePickerConfigurationTreeSource
{
    /// <summary>
    /// Gets or sets the object type (e.g., content, media, member).
    /// </summary>
    [JsonPropertyName("type")]
    [DataMember(Name = "type")]
    public string? ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the XPath query for the start node.
    /// </summary>
    [JsonPropertyName("query")]
    [DataMember(Name = "query")]
    public string? StartNodeQuery { get; set; }

    /// <summary>
    /// Gets or sets the dynamic root configuration.
    /// </summary>
    [DataMember(Name = "dynamicRoot")]
    public DynamicRoot? DynamicRoot { get; set; }

    /// <summary>
    /// Gets or sets the start node ID.
    /// </summary>
    [JsonPropertyName("id")]
    [DataMember(Name = "id")]
    public Guid? StartNodeId { get; set; }
}

/// <summary>
/// Represents a dynamic root configuration for the multi-node picker.
/// </summary>
[DataContract]
public class DynamicRoot
{
    /// <summary>
    /// Gets or sets the origin alias for the dynamic root.
    /// </summary>
    [DataMember(Name = "originAlias")]
    public string OriginAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin key for the dynamic root.
    /// </summary>
    [DataMember(Name = "originKey")]
    public Guid? OriginKey { get; set; }

    /// <summary>
    /// Gets or sets the query steps for traversing the content tree.
    /// </summary>
    [DataMember(Name = "querySteps")]
    public QueryStep[] QuerySteps { get; set; } = Array.Empty<QueryStep>();
}

/// <summary>
/// Represents a query step for dynamic root traversal in the multi-node picker.
/// </summary>
[DataContract]
public class QueryStep
{
    /// <summary>
    /// Gets or sets the alias of the query step.
    /// </summary>
    [DataMember(Name = "alias")]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type keys to filter by.
    /// </summary>
    [DataMember(Name = "anyOfDocTypeKeys")]
    public IEnumerable<Guid> AnyOfDocTypeKeys { get; set; } = Array.Empty<Guid>();
}

