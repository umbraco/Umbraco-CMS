namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     Represents a query step configuration that defines how to traverse or filter the content tree during dynamic root resolution.
/// </summary>
public class DynamicRootQueryStep
{
    /// <summary>
    /// Empty means all Doctypes
    /// </summary>
    public IEnumerable<Guid> AnyOfDocTypeKeys { get; set; } = Array.Empty<Guid>();

    /// <summary>
    ///     Gets or sets the alias identifying the type of query step to execute (e.g., "NearestAncestorOrSelf", "FurthestDescendantOrSelf").
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}
