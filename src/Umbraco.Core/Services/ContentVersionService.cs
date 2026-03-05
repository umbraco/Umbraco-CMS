using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing content versions, including retrieval, cleanup, and rollback operations.
/// </summary>
internal sealed class ContentVersionService : ContentVersionServiceBase<IContent>, IContentVersionService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentVersionService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    /// <param name="contentVersionCleanupPolicy">The content version cleanup policy.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    public ContentVersionService(
        ILogger<ContentVersionService> logger,
        IDocumentVersionRepository contentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(
            logger,
            contentVersionRepository,
            contentVersionCleanupPolicy,
            scopeProvider,
            eventMessagesFactory,
            auditService,
            languageRepository,
            entityService,
            contentService,
            userIdKeyResolver)
    {
    /// <summary>
    /// Gets a content version by its ID.
    /// </summary>
    /// <param name="versionId">The version ID.</param>
    /// <returns>The content version metadata, or null if not found.</returns>
    /// <inheritdoc />
    /// <inheritdoc />
    /// <inheritdoc />
    /// <inheritdoc />
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override DeletingVersionsNotification<IContent> DeletingVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ContentDeletingVersionsNotification(id, messages, specificVersion);

    protected override DeletedVersionsNotification<IContent> DeletedVersionsNotification(int id, EventMessages messages, int specificVersion)
        => new ContentDeletedVersionsNotification(id, messages, specificVersion);
}
