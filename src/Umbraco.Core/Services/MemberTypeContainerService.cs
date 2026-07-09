using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Locking;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing member type containers (folders).
/// </summary>
/// <remarks>
///     Member type containers allow organizing member types into a hierarchical folder structure.
/// </remarks>
internal sealed class MemberTypeContainerService : EntityTypeContainerService<IMemberType, IMemberTypeContainerRepository>, IMemberTypeContainerService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeContainerService" /> class.
    /// </summary>
    /// <param name="provider">The scope provider for unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="entityContainerRepository">The repository for member type container data access.</param>
    /// <param name="auditService">The audit service for recording audit entries.</param>
    /// <param name="entityRepository">The entity repository for entity operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    public MemberTypeContainerService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMemberTypeContainerRepository entityContainerRepository,
        IAuditService auditService,
        IEntityRepository entityRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, entityContainerRepository, auditService, entityRepository, userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.MemberType;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.MemberTypeContainer;

    /// <inheritdoc />
    protected override int[] ReadLockIds => MemberTypeLocks.ReadLockIds;

    /// <inheritdoc />
    protected override int[] WriteLockIds => MemberTypeLocks.WriteLockIds;
}
