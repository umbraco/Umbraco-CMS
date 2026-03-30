using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Represents a service that provides functionality for importing dictionary items into the system.
/// </summary>
public interface IDictionaryItemImportService
{
    /// <summary>
    /// Asynchronously imports a dictionary item from a temporary UDT (Umbraco Dictionary Transfer) file.
    /// </summary>
    /// <param name="temporaryFileKey">The unique key identifying the temporary UDT file to import from.</param>
    /// <param name="parentKey">The optional key of the parent dictionary item under which the imported item will be placed. If <c>null</c>, the item is imported at the root level.</param>
    /// <param name="userKey">The key of the user performing the import operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The result contains an <see cref="Attempt{T}"/> with the imported <see cref="IDictionaryItem"/> (or <c>null</c> if unsuccessful) and the <see cref="DictionaryImportOperationStatus"/> indicating the outcome.</returns>
    Task<Attempt<IDictionaryItem?, DictionaryImportOperationStatus>> ImportDictionaryItemFromUdtFileAsync(Guid temporaryFileKey, Guid? parentKey, Guid userKey);
}
