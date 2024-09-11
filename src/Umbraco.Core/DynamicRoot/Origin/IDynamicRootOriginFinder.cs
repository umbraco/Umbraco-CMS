namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
/// Supports finding the Origin For a given query
/// </summary>
public interface IDynamicRootOriginFinder
{
    Guid? FindOriginKey(DynamicRootNodeQuery dynamicRootNodeQuery);
}
