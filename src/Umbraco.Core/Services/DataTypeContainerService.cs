using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing data type containers (folders).
/// </summary>
internal sealed class DataTypeContainerService : EntityTypeContainerService<IDataType, IDataTypeContainerRepository>, IDataTypeContainerService
{
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
    public DataTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDataTypeContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.DataType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DataTypeContainer;

    /// <inheritdoc />
    /// <remarks>Data types do not have read/write locks (yet).</remarks>
    protected override int[] ReadLockIds => [];

    /// <inheritdoc />
    /// <remarks>Data types do not have read/write locks (yet).</remarks>
    protected override int[] WriteLockIds => [];
}
