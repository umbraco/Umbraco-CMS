namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public interface IDynamicRootOrigin
{
    Guid? FindOriginKey(DynamicRootNodeSelector dynamicRootNodeSelector);
}
