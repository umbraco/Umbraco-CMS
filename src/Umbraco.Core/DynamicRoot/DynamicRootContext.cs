namespace Umbraco.Cms.Core.DynamicRoot;

public struct DynamicRootContext
{
    public required Guid? CurrentKey { get; set; }

    public required Guid ParentKey { get; set; }
}
