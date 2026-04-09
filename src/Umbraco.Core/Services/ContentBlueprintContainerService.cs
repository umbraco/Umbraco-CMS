using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing content blueprint containers (folders).
/// </summary>
internal sealed class ContentBlueprintContainerService : EntityTypeContainerService<IContent, IDocumentBlueprintContainerRepository>, IContentBlueprintContainerService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBlueprintContainerService"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="entityContainerRepository">The document blueprint container repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    public ContentBlueprintContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentBlueprintContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentBlueprint;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentBlueprintContainer;

    /// <inheritdoc />
    protected override int[] ReadLockIds => new [] { Constants.Locks.ContentTree };

    /// <inheritdoc />
    protected override int[] WriteLockIds => ReadLockIds;
}
