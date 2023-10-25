namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public interface IStartNodeOriginFinder
{
    Guid? FindOriginKey(DynamicRootNodeSelector dynamicRootNodeSelector);
}
