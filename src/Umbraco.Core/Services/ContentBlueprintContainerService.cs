using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal sealed class ContentBlueprintContainerService : EntityTypeContainerService<IContent, IDocumentBlueprintContainerRepository>, IContentBlueprintContainerService
{
    public ContentBlueprintContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentBlueprintContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditRepository, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentBlueprint;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentBlueprintContainer;

    protected override int[] ReadLockIds => new [] { Constants.Locks.ContentTree };

    protected override int[] WriteLockIds => ReadLockIds;
}
