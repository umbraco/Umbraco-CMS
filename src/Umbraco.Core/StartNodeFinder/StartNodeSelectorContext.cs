namespace Umbraco.Cms.Core.StartNodeFinder;

public struct StartNodeSelectorContext
{
    public required Guid? CurrentKey { get; set; }
    public required Guid ParentKey { get; set; }
}
