namespace Umbraco.Cms.Core.DynamicRoot;

/// <summary>
/// Supports finding content roots for pickers (like MNTP) in a dynamic fashion
/// </summary>
public interface IDynamicRootService
{
    /// <summary>
    ///     Gets the dynamic root content keys based on the specified query.
    /// </summary>
    /// <param name="dynamicRootNodeQuery">The query specifying the origin, context, and optional query steps for finding dynamic roots.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of unique identifiers for the resolved dynamic root content items.</returns>
    Task<IEnumerable<Guid>> GetDynamicRootsAsync(DynamicRootNodeQuery dynamicRootNodeQuery);
}
