using System.Text.Json.Serialization;

namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Describes the health of a search index.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
    /// <summary>
    /// The index is present and usable.
    /// </summary>
    Healthy,

    /// <summary>
    /// The index is currently being rebuilt.
    /// </summary>
    Rebuilding,

    /// <summary>
    /// The index exists but could not be read correctly.
    /// </summary>
    Corrupted,

    /// <summary>
    /// The index exists but contains no documents.
    /// </summary>
    Empty,

    /// <summary>
    /// The index health could not be determined.
    /// </summary>
    Unknown,
}
