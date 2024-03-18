using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal sealed class DataTypeContainerService : EntityTypeContainerService<IDataType, IDataTypeContainerRepository>, IDataTypeContainerService
{
    public DataTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDataTypeContainerRepository entityContainerRepository,
        IAuditRepository auditRepository,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditRepository, entityRepository, userIdKeyResolver)
    {
    }

    protected override Guid ContainedObjectType => Constants.ObjectTypes.DataType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DataTypeContainer;

    // data types do not have read/write locks (yet)
    protected override int[] ReadLockIds => [];

    // data types do not have read/write locks (yet)
    protected override int[] WriteLockIds => [];
}
