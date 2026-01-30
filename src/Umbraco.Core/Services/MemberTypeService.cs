using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing member types in Umbraco.
/// </summary>
/// <remarks>
///     This service handles operations for member types, which define the structure and properties
///     available for members in the system. It extends the base content type service functionality
///     with member-specific behavior.
/// </remarks>
public class MemberTypeService : ContentTypeServiceBase<IMemberTypeRepository, IMemberType>, IMemberTypeService
{
    private readonly IMemberTypeRepository _memberTypeRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="memberService">The member service for member-related operations.</param>
    /// <param name="memberTypeRepository">The repository for member type operations.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="entityContainerRepository">The repository for member type container operations.</param>
    /// <param name="entityRepository">The repository for entity operations.</param>
    /// <param name="eventAggregator">The event aggregator for publishing notifications.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    public MemberTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMemberService memberService,
        IMemberTypeRepository memberTypeRepository,
        IAuditService auditService,
        IMemberTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            memberTypeRepository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
        MemberService = memberService;
        _memberTypeRepository = memberTypeRepository;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="memberService">The member service for member-related operations.</param>
    /// <param name="memberTypeRepository">The repository for member type operations.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    /// <param name="entityContainerRepository">The repository for member type container operations.</param>
    /// <param name="entityRepository">The repository for entity operations.</param>
    /// <param name="eventAggregator">The event aggregator for publishing notifications.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public MemberTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMemberService memberService,
        IMemberTypeRepository memberTypeRepository,
        IAuditRepository auditRepository,
        IMemberTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            memberService,
            memberTypeRepository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypeService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="memberService">The member service for member-related operations.</param>
    /// <param name="memberTypeRepository">The repository for member type operations.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    /// <param name="entityContainerRepository">The repository for member type container operations.</param>
    /// <param name="entityRepository">The repository for entity operations.</param>
    /// <param name="eventAggregator">The event aggregator for publishing notifications.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public MemberTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMemberService memberService,
        IMemberTypeRepository memberTypeRepository,
        IAuditService auditService,
        IAuditRepository auditRepository,
        IMemberTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            memberService,
            memberTypeRepository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
    }

    // beware! order is important to avoid deadlocks
    /// <inheritdoc />
    protected override int[] ReadLockIds { get; } = { Constants.Locks.MemberTypes };

    /// <inheritdoc />
    protected override int[] WriteLockIds { get; } = { Constants.Locks.MemberTree, Constants.Locks.MemberTypes };

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.MemberType;

    /// <summary>
    ///     Gets the member service used for member-related operations.
    /// </summary>
    private IMemberService MemberService { get; }

    /// <summary>
    ///     Gets the alias of the default member type.
    /// </summary>
    /// <returns>
    ///     The alias of the member type named "Member" if it exists; otherwise, the alias of the first available member type.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when no member types are available in the system.</exception>
    public string GetDefault()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);

            using (IEnumerator<IMemberType> e = _memberTypeRepository.GetMany(Array.Empty<int>()).GetEnumerator())
            {
                if (e.MoveNext() == false)
                {
                    throw new InvalidOperationException("No member types could be resolved");
                }

                var first = e.Current.Alias;
                var current = true;
                while (e.Current.Alias.InvariantEquals("Member") == false && (current = e.MoveNext()))
                {
                }

                return current ? e.Current.Alias : first;
            }
        }
    }

    /// <summary>
    ///     Deletes all members that use the specified member type IDs.
    /// </summary>
    /// <param name="typeIds">The IDs of the member types whose members should be deleted.</param>
    protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
    {
        foreach (var typeId in typeIds)
        {
            MemberService.DeleteMembersOfType(typeId);
        }
    }

    #region Notifications

    /// <inheritdoc />
    protected override SavingNotification<IMemberType> GetSavingNotification(
        IMemberType item,
        EventMessages eventMessages) => new MemberTypeSavingNotification(item, eventMessages);

    /// <inheritdoc />
    protected override SavingNotification<IMemberType> GetSavingNotification(
        IEnumerable<IMemberType> items,
        EventMessages eventMessages) => new MemberTypeSavingNotification(items, eventMessages);

    /// <inheritdoc />
    protected override SavedNotification<IMemberType> GetSavedNotification(
        IMemberType item,
        EventMessages eventMessages) => new MemberTypeSavedNotification(item, eventMessages);

    /// <inheritdoc />
    protected override SavedNotification<IMemberType> GetSavedNotification(
        IEnumerable<IMemberType> items,
        EventMessages eventMessages) => new MemberTypeSavedNotification(items, eventMessages);

    /// <inheritdoc />
    protected override DeletingNotification<IMemberType> GetDeletingNotification(
        IMemberType item,
        EventMessages eventMessages) => new MemberTypeDeletingNotification(item, eventMessages);

    /// <inheritdoc />
    protected override DeletingNotification<IMemberType> GetDeletingNotification(
        IEnumerable<IMemberType> items,
        EventMessages eventMessages) => new MemberTypeDeletingNotification(items, eventMessages);

    /// <inheritdoc />
    protected override DeletedNotification<IMemberType> GetDeletedNotification(
        IEnumerable<IMemberType> items,
        EventMessages eventMessages) => new MemberTypeDeletedNotification(items, eventMessages);

    /// <inheritdoc />
    protected override MovingNotification<IMemberType> GetMovingNotification(
        MoveEventInfo<IMemberType> moveInfo,
        EventMessages eventMessages) => new MemberTypeMovingNotification(moveInfo, eventMessages);

    /// <inheritdoc />
    protected override MovedNotification<IMemberType> GetMovedNotification(
        IEnumerable<MoveEventInfo<IMemberType>> moveInfo, EventMessages eventMessages) =>
        new MemberTypeMovedNotification(moveInfo, eventMessages);

    /// <inheritdoc />
    protected override ContentTypeChangeNotification<IMemberType> GetContentTypeChangedNotification(
        IEnumerable<ContentTypeChange<IMemberType>> changes, EventMessages eventMessages) =>
        new MemberTypeChangedNotification(changes, eventMessages);

    /// <inheritdoc />
    protected override ContentTypeRefreshNotification<IMemberType> GetContentTypeRefreshedNotification(
        IEnumerable<ContentTypeChange<IMemberType>> changes, EventMessages eventMessages) =>
        new MemberTypeRefreshedNotification(changes, eventMessages);

    #endregion
}
