using Umbraco.Cms.Core.DynamicRoot.QuerySteps;

namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
/// Specifies origin and context data with optional query steps to find Dynamic Roots
/// </summary>
public class DynamicRootNodeQuery
{
    public required string OriginAlias { get; set; }

    public Guid? OriginKey { get; set; }

    public required DynamicRootContext Context { get; set; }

    public IEnumerable<DynamicRootQueryStep> QuerySteps { get; set; } = Array.Empty<DynamicRootQueryStep>();
}
