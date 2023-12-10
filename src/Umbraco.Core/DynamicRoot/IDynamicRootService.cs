namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
/// Supports finding content roots for pickers (like MNTP) in a dynamic fashion
/// </summary>
public interface IDynamicRootService
{
    Task<IEnumerable<Guid>> GetDynamicRootsAsync(DynamicRootNodeQuery dynamicRootNodeQuery);
}
