using Umbraco.Cms.Core.DynamicRoot.QuerySteps;

namespace Umbraco.Cms.Core.DynamicRoot;

public class DynamicRootNodeSelector
{
    public required string OriginAlias { get; set; }
    public Guid? OriginKey { get; set; }

    public required DynamicRootSelectorContext Context { get; set; }

    public IEnumerable<DynamicRootQueryStep> QuerySteps { get; set; } = Array.Empty<DynamicRootQueryStep>();
}
