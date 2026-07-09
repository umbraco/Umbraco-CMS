using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides a generic base interface for content services.
/// </summary>
/// <typeparam name="TItem">The type of content item managed by this service.</typeparam>
public interface IContentServiceBase<TItem> : IContentServiceBase
    where TItem : class, IContentBase
{
    /// <summary>
    ///     Gets a content item by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <returns>The content item, or <c>null</c> if not found.</returns>
    TItem? GetById(Guid key);

    /// <summary>
    ///     Saves a collection of content items.
    /// </summary>
    /// <param name="contents">The content items to save.</param>
    /// <param name="userId">The identifier of the user performing the save operation.</param>
    /// <returns>An attempt containing the operation result.</returns>
    Attempt<OperationResult?> Save(IEnumerable<TItem> contents, int userId = Constants.Security.SuperUserId);
}

/// <summary>
///     Placeholder for sharing logic between the content, media (and member) services
///     TODO: Start sharing the logic!
/// </summary>
public interface IContentServiceBase : IService
{
    /// <summary>
    ///     Checks/fixes the data integrity of node paths/levels stored in the database
    /// </summary>
    ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
}
