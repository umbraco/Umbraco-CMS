namespace Umbraco.Cms.Core.DynamicRoot;

public struct DynamicRootSelectorContext
{
    public required Guid? CurrentKey { get; set; }
    public required Guid ParentKey { get; set; }
}
