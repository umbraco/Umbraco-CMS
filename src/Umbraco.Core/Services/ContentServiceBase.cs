using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Abstract base class for content-related services providing shared infrastructure.
/// </summary>
public abstract class ContentServiceBase : RepositoryService
{
    protected readonly IDocumentRepository DocumentRepository;
    protected readonly IAuditService AuditService;
    protected readonly IUserIdKeyResolver UserIdKeyResolver;

    protected ContentServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        DocumentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        AuditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        UserIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
    }

    /// <summary>
    /// Records an audit entry for a content operation (synchronous).
    /// </summary>
    /// <remarks>
    /// Uses ConfigureAwait(false) to avoid capturing synchronization context and prevent deadlocks.
    /// TODO: Replace with sync overloads when IAuditService.Add and IUserIdKeyResolver.Get are available.
    /// </remarks>
    protected void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        // Use ConfigureAwait(false) to avoid context capture and potential deadlocks
        Guid userKey = UserIdKeyResolver.GetAsync(userId).ConfigureAwait(false).GetAwaiter().GetResult();

        AuditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Records an audit entry for a content operation asynchronously.
    /// </summary>
    protected async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await UserIdKeyResolver.GetAsync(userId).ConfigureAwait(false);

        await AuditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters).ConfigureAwait(false);
    }
}
