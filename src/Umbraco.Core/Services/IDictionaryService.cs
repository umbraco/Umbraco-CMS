namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides utility services for dictionary items.
/// </summary>
public interface IDictionaryService
{
    /// <summary>
    ///     Calculates the hierarchical path for a dictionary item.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent dictionary item, or <c>null</c> for root items.</param>
    /// <param name="sourceId">The source identifier of the dictionary item.</param>
    /// <returns>The calculated path string.</returns>
    string CalculatePath(Guid? parentId, int sourceId);
}
