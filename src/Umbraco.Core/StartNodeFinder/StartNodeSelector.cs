using Umbraco.Cms.Core.StartNodeFinder.Filters;

namespace Umbraco.Cms.Core.StartNodeFinder;

public class StartNodeSelector
{
    public required string OriginAlias { get; set; }
    public Guid? OriginKey { get; set; }

    public required StartNodeSelectorContext Context { get; set; }

    public IEnumerable<StartNodeFilter> Filter { get; set; } = Array.Empty<StartNodeFilter>();
}
