using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a dynamic root configuration, being a start node resolved against the content being edited rather
/// than fixed on the data type.
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
