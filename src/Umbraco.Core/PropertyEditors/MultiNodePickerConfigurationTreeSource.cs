using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the 'startNode' value for the <see cref="MultiNodePickerConfiguration" />
/// </summary>
[Obsolete("The multi node tree picker is replaced by the dedicated document, media, element and member pickers. Scheduled for removal in Umbraco 22.")]
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
