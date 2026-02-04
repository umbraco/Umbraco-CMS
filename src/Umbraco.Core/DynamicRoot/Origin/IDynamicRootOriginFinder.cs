namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
/// Supports finding the Origin For a given query
/// </summary>
public interface IDynamicRootOriginFinder
{
    /// <summary>
    ///     Attempts to find the origin content key based on the specified query.
    /// </summary>
    /// <param name="dynamicRootNodeQuery">The query containing the origin alias, optional origin key, and context information.</param>
    /// <returns>The unique identifier of the origin content, or <c>null</c> if this finder does not support the query or cannot find the origin.</returns>
    Guid? FindOriginKey(DynamicRootNodeQuery dynamicRootNodeQuery);
}
