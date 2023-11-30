using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentPublishingService
{
    /// <summary>
    ///     Publishes a single content item.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Status of the publish operation.</returns>
    Task<Attempt<ContentPublishingOperationStatus>> PublishAsync(Guid key, IEnumerable<string> cultures, Guid userKey);

    /// <summary>
    ///     Publishes a content branch.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="cultures">The cultures to publish.</param>
    /// <param name="force">A value indicating whether to force-publish content that is not already published.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>A dictionary of attempted content item keys and their corresponding publishing status.</returns>
    Task<Attempt<IDictionary<Guid, ContentPublishingOperationStatus>>> PublishBranchAsync(Guid key, IEnumerable<string> cultures, bool force, Guid userKey);

    /// <summary>
    ///     Unpublishes a single content item.
    /// </summary>
    /// <param name="key">The key of the root content.</param>
    /// <param name="culture">The culture to unpublish. Use null to unpublish all cultures.</param>
    /// <param name="userKey">The identifier of the user performing the operation.</param>
    /// <returns>Status of the publish operation.</returns>
    Task<Attempt<ContentPublishingOperationStatus>> UnpublishAsync(Guid key, string? culture, Guid userKey);
}
