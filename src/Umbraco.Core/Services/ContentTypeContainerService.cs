using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing content type containers (folders).
/// </summary>
internal sealed class ContentTypeContainerService : EntityTypeContainerService<IContentType, IDocumentTypeContainerRepository>, IContentTypeContainerService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeContainerService"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="entityContainerRepository">The document type container repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    public ContentTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentTypeContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    /// <inheritdoc />
    protected override int[] ReadLockIds => ContentTypeLocks.ReadLockIds;

    /// <inheritdoc />
    protected override int[] WriteLockIds => ContentTypeLocks.WriteLockIds;
}
