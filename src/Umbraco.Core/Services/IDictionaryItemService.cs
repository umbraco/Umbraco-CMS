using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDictionaryItemService
{
    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="Guid" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    Task<IDictionaryItem?> GetAsync(Guid id);

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by  by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    Task<IDictionaryItem?> GetAsync(string key);

    /// <summary>
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their <see cref="Guid" /> ids
    /// </summary>
    /// <param name="ids">Ids of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    Task<IEnumerable<IDictionaryItem>> GetManyAsync(params Guid[] ids);

    /// <summary>
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their keys
    /// </summary>
    /// <param name="keys">Keys of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    Task<IEnumerable<IDictionaryItem>> GetManyAsync(params string[] keys);

    /// <summary>
    ///     Gets a list of children for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    Task<IEnumerable<IDictionaryItem>> GetChildrenAsync(Guid parentId);

    /// <summary>
    ///     Gets a list of descendants for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent, null will return all dictionary items</param>
    /// <param name="filter">An optional filter, which will limit the results to only those dictionary items whose key starts with the filter value.</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    Task<IEnumerable<IDictionaryItem>> GetDescendantsAsync(Guid? parentId, string? filter = null);

    /// <summary>
    ///     Gets the root/top <see cref="IDictionaryItem" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    Task<IEnumerable<IDictionaryItem>> GetAtRootAsync();

    /// <summary>
    ///     Checks if a <see cref="IDictionaryItem" /> with given key exists
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>True if a <see cref="IDictionaryItem" /> exists, otherwise false</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    ///     Creates and saves a new dictionary item and assigns translations to all applicable languages if specified
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to create</param>
    /// <param name="userKey">Key of the user saving the dictionary item</param>
    /// <returns></returns>
    Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> CreateAsync(IDictionaryItem dictionaryItem, Guid userKey);

    /// <summary>
    ///     Updates an existing <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to update</param>
    /// <param name="userKey">Key of the user saving the dictionary item</param>
    Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> UpdateAsync(IDictionaryItem dictionaryItem, Guid userKey);

    /// <summary>
    ///     Deletes a <see cref="IDictionaryItem" /> object and its related translations
    ///     as well as its children.
    /// </summary>
    /// <param name="id">The ID of the <see cref="IDictionaryItem" /> to delete</param>
    /// <param name="userKey">Key of the user deleting the dictionary item</param>
    Task<Attempt<IDictionaryItem?, DictionaryItemOperationStatus>> DeleteAsync(Guid id, Guid userKey);

    /// <summary>
    ///     Moves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to move</param>
    /// <param name="parentId">Id of the new <see cref="IDictionaryItem" /> parent, null if the item should be moved to the root</param>
    /// <param name="userKey">Key of the user moving the dictionary item</param>
    Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> MoveAsync(IDictionaryItem dictionaryItem, Guid? parentId, Guid userKey);

    Task<int> CountChildrenAsync(Guid parentId);
    Task<int> CountRootAsync();

    /// <summary>
    /// Gets the dictionary items in a paged manner.
    /// Currently implements the paging in memory on the itenkey property because the underlying repository does not support paging yet
    /// </summary>
    Task<PagedModel<IDictionaryItem>> GetPagedAsync(Guid? parentId, int skip, int take);
}
