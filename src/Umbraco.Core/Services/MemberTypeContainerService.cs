using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

internal sealed class MemberTypeContainerService : EntityTypeContainerService<IMemberType, IMemberTypeContainerRepository>, IMemberTypeContainerService
{
    public MemberTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMemberTypeContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditRepository, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.MemberType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MemberTypeContainer;

    protected override int[] ReadLockIds => MemberTypeLocks.ReadLockIds;

    protected override int[] WriteLockIds => MemberTypeLocks.WriteLockIds;
}
