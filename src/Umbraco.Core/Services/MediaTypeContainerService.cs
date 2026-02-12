using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing media type containers (folders).
/// </summary>
/// <remarks>
///     Media type containers allow organizing media types into a hierarchical folder structure.
/// </remarks>
internal sealed class MediaTypeContainerService : EntityTypeContainerService<IMediaType, IMediaTypeContainerRepository>, IMediaTypeContainerService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeContainerService" /> class.
    /// </summary>
    /// <param name="provider">The scope provider for unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="entityContainerRepository">The repository for media type container data access.</param>
    /// <param name="auditService">The audit service for recording audit entries.</param>
    /// <param name="entityRepository">The entity repository for entity operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    public MediaTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaTypeContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MediaTypeContainer;

    /// <inheritdoc />
    protected override int[] ReadLockIds => MediaTypeLocks.ReadLockIds;

    /// <inheritdoc />
    protected override int[] WriteLockIds => MediaTypeLocks.WriteLockIds;
}
