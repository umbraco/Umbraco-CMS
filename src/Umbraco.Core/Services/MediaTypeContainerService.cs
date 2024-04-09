using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

internal sealed class MediaTypeContainerService : EntityTypeContainerService<IMediaType, IMediaTypeContainerRepository>, IMediaTypeContainerService
{
    public MediaTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaTypeContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditRepository, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MediaTypeContainer;

    protected override int[] ReadLockIds => MediaTypeLocks.ReadLockIds;

    protected override int[] WriteLockIds => MediaTypeLocks.WriteLockIds;
}
