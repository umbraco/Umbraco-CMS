using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a query step for dynamic root traversal.
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
