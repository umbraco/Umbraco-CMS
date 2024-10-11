using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedContentTypeCache
{
    /// <summary>
    ///     Clears the entire cache.
    /// </summary>
    public void ClearAll();

    /// <summary>
    ///     Clears a cached content type.
    /// </summary>
    /// <param name="id">An identifier.</param>
    public void ClearContentType(int id);

    /// <summary>
    ///     Clears cached content types.
    /// </summary>
    /// <param name="ids">ContentType IDs to clear</param>
    public void ClearContentTypes(IEnumerable<int> ids)
    {
        foreach (var id in ids)
        {
            ClearContentType(id);
        }
    }

    /// <summary>
    ///     Clears all cached content types referencing a data type.
    /// </summary>
    /// <param name="id">A data type identifier.</param>
    public void ClearDataType(int id);

    /// <summary>
    /// Clears all cached content types referencing a data type.
    /// </summary>
    /// <param name="id">The data type id to remove content types by</param>
    /// <returns>The removed content types</returns>
    public IEnumerable<IPublishedContentType> ClearByDataTypeId(int id)
    {
        ClearDataType(id);
        return [];
    }

    /// <summary>
    ///     Gets a published content type.
    /// </summary>
    /// <param name="itemType">An item type.</param>
    /// <param name="key">An key.</param>
    /// <returns>The published content type corresponding to the item key.</returns>
    public IPublishedContentType Get(PublishedItemType itemType, Guid key);

    /// <summary>
    ///     Gets a published content type.
    /// </summary>
    /// <param name="itemType">An item type.</param>
    /// <param name="alias">An alias.</param>
    /// <returns>The published content type corresponding to the item type and alias.</returns>
    public IPublishedContentType Get(PublishedItemType itemType, string alias);

    /// <summary>
    ///     Gets a published content type.
    /// </summary>
    /// <param name="itemType">An item type.</param>
    /// <param name="id">An identifier.</param>
    /// <returns>The published content type corresponding to the item type and identifier.</returns>
    public IPublishedContentType Get(PublishedItemType itemType, int id);
}
