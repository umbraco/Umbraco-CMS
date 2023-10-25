namespace Umbraco.Cms.Core.DynamicRoot;

public interface IDynamicRootService
{
    IEnumerable<Guid> GetDynamicRoots(DynamicRootNodeSelector dynamicRootNodeSelector);
}
