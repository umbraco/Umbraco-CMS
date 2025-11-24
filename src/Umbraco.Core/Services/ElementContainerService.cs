using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal sealed class ElementContainerService : EntityTypeContainerService<IElement, IElementContainerRepository>, IElementContainerService
{
    public ElementContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IElementContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.Element;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.ElementContainer;

    protected override int[] ReadLockIds => new [] { Constants.Locks.ElementTree };

    protected override int[] WriteLockIds => ReadLockIds;
}
