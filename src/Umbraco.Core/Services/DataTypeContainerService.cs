using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing data type containers (folders).
/// </summary>
internal sealed class DataTypeContainerService : EntityTypeContainerService<IDataType, IDataTypeContainerRepository>, IDataTypeContainerService
{
    private readonly IDataTypeRepository _dataTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeContainerService"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="entityContainerRepository">The data type container repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="dataTypeRepository">The data type repository.</param>
    public DataTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDataTypeContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityService entityService,
        IDataTypeRepository dataTypeRepository)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver, entityService)
        => _dataTypeRepository = dataTypeRepository;

    /// <inheritdoc />
    protected override IDataType? GetContainedEntity(int id) => _dataTypeRepository.Get(id);

    /// <inheritdoc />
    protected override void SaveContainedEntity(IDataType entity) => _dataTypeRepository.Save(entity);

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.DataType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DataTypeContainer;

    /// <inheritdoc />
    protected override int[] ReadLockIds => DataTypeLocks.ReadLockIds;

    /// <inheritdoc />
    protected override int[] WriteLockIds => DataTypeLocks.WriteLockIds;
}
