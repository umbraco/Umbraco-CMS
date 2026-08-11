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
    private readonly IDocumentBlueprintRepository _documentBlueprintRepository;

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
    /// <param name="entityService">The entity service.</param>
    /// <param name="documentBlueprintRepository">The document blueprint repository.</param>
    public ContentBlueprintContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentBlueprintContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityService entityService,
        IDocumentBlueprintRepository documentBlueprintRepository)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver, entityService)
        => _documentBlueprintRepository = documentBlueprintRepository;

    /// <inheritdoc />
    protected override IContent? GetContainedEntity(int id)
    {
        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint is not null)
        {
            // the repository does not set this, see also ContentService.GetBlueprintById
            blueprint.Blueprint = true;
        }

        return blueprint;
    }

    /// <inheritdoc />
    protected override void SaveContainedEntity(IContent entity)
    {
        entity.Blueprint = true;
        _documentBlueprintRepository.Save(entity);
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
