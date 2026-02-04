using Umbraco.Cms.Core.DynamicRoot.QuerySteps;

namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
/// Specifies origin and context data with optional query steps to find Dynamic Roots
/// </summary>
public class DynamicRootNodeQuery
{
    /// <summary>
    ///     Gets or sets the alias identifying the type of origin finder to use (e.g., "ByKey", "Root", "Current", "Parent", "Site", "ContentRoot").
    /// </summary>
    public required string OriginAlias { get; set; }

    /// <summary>
    ///     Gets or sets the optional unique identifier for the origin content item, used by certain origin finders like "ByKey".
    /// </summary>
    public Guid? OriginKey { get; set; }

    /// <summary>
    ///     Gets or sets the context containing current and parent content information for resolving the dynamic root.
    /// </summary>
    public required DynamicRootContext Context { get; set; }

    /// <summary>
    ///     Gets or sets the collection of query steps to apply after finding the origin to further filter or traverse the content tree.
    /// </summary>
    public IEnumerable<DynamicRootQueryStep> QuerySteps { get; set; } = Array.Empty<DynamicRootQueryStep>();
}
