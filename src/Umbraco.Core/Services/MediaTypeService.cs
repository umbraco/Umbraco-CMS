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
using Umbraco.Cms.Core.Services.Locking;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the Media Type Service, which provides operations for managing <see cref="IMediaType"/> entities.
/// </summary>
public class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType>, IMediaTypeService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeService"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="ICoreScopeProvider"/> for database scope management.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers.</param>
    /// <param name="eventMessagesFactory">The <see cref="IEventMessagesFactory"/> for creating event messages.</param>
    /// <param name="mediaService">The <see cref="IMediaService"/> for media operations.</param>
    /// <param name="mediaTypeRepository">The <see cref="IMediaTypeRepository"/> for media type persistence.</param>
    /// <param name="auditService">The <see cref="IAuditService"/> for audit logging.</param>
    /// <param name="entityContainerRepository">The <see cref="IMediaTypeContainerRepository"/> for media type container operations.</param>
    /// <param name="entityRepository">The <see cref="IEntityRepository"/> for entity operations.</param>
    /// <param name="eventAggregator">The <see cref="IEventAggregator"/> for publishing events.</param>
    /// <param name="userIdKeyResolver">The <see cref="IUserIdKeyResolver"/> for resolving user IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    public MediaTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaService mediaService,
        IMediaTypeRepository mediaTypeRepository,
        IAuditService auditService,
        IMediaTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaTypeRepository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
        MediaService = mediaService;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeService"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="ICoreScopeProvider"/> for database scope management.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers.</param>
    /// <param name="eventMessagesFactory">The <see cref="IEventMessagesFactory"/> for creating event messages.</param>
    /// <param name="mediaService">The <see cref="IMediaService"/> for media operations.</param>
    /// <param name="mediaTypeRepository">The <see cref="IMediaTypeRepository"/> for media type persistence.</param>
    /// <param name="auditRepository">The audit repository (obsolete, not used).</param>
    /// <param name="entityContainerRepository">The <see cref="IMediaTypeContainerRepository"/> for media type container operations.</param>
    /// <param name="entityRepository">The <see cref="IEntityRepository"/> for entity operations.</param>
    /// <param name="eventAggregator">The <see cref="IEventAggregator"/> for publishing events.</param>
    /// <param name="userIdKeyResolver">The <see cref="IUserIdKeyResolver"/> for resolving user IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public MediaTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaService mediaService,
        IMediaTypeRepository mediaTypeRepository,
        IAuditRepository auditRepository,
        IMediaTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaService,
            mediaTypeRepository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaTypeService"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="ICoreScopeProvider"/> for database scope management.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers.</param>
    /// <param name="eventMessagesFactory">The <see cref="IEventMessagesFactory"/> for creating event messages.</param>
    /// <param name="mediaService">The <see cref="IMediaService"/> for media operations.</param>
    /// <param name="mediaTypeRepository">The <see cref="IMediaTypeRepository"/> for media type persistence.</param>
    /// <param name="auditService">The <see cref="IAuditService"/> for audit logging.</param>
    /// <param name="auditRepository">The audit repository (obsolete, not used).</param>
    /// <param name="entityContainerRepository">The <see cref="IMediaTypeContainerRepository"/> for media type container operations.</param>
    /// <param name="entityRepository">The <see cref="IEntityRepository"/> for entity operations.</param>
    /// <param name="eventAggregator">The <see cref="IEventAggregator"/> for publishing events.</param>
    /// <param name="userIdKeyResolver">The <see cref="IUserIdKeyResolver"/> for resolving user IDs.</param>
    /// <param name="contentTypeFilters">The collection of content type filters.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public MediaTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaService mediaService,
        IMediaTypeRepository mediaTypeRepository,
        IAuditService auditService,
        IAuditRepository auditRepository,
        IMediaTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaService,
            mediaTypeRepository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
    }

    /// <inheritdoc />
    protected override int[] ReadLockIds => MediaTypeLocks.ReadLockIds;

    /// <inheritdoc />
    protected override int[] WriteLockIds => MediaTypeLocks.WriteLockIds;

    /// <inheritdoc />
    protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaType;

    /// <summary>
    ///     Gets the <see cref="IMediaService"/> used for media operations.
    /// </summary>
    private IMediaService MediaService { get; }

    /// <inheritdoc />
    protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
    {
        foreach (var typeId in typeIds)
        {
            MediaService.DeleteMediaOfType(typeId);
        }
    }

    /// <inheritdoc />
    protected override bool CanDelete(IMediaType item)
        => item.IsSystemMediaType() is false;

    #region Notifications

    /// <inheritdoc />
    protected override SavingNotification<IMediaType> GetSavingNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeSavingNotification(item, eventMessages);

    /// <inheritdoc />
    protected override SavingNotification<IMediaType> GetSavingNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeSavingNotification(items, eventMessages);

    /// <inheritdoc />
    protected override SavedNotification<IMediaType> GetSavedNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeSavedNotification(item, eventMessages);

    /// <inheritdoc />
    protected override SavedNotification<IMediaType> GetSavedNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeSavedNotification(items, eventMessages);

    /// <inheritdoc />
    protected override DeletingNotification<IMediaType> GetDeletingNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeDeletingNotification(item, eventMessages);

    /// <inheritdoc />
    protected override DeletingNotification<IMediaType> GetDeletingNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeDeletingNotification(items, eventMessages);

    /// <inheritdoc />
    protected override DeletedNotification<IMediaType> GetDeletedNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeDeletedNotification(items, eventMessages);

    /// <inheritdoc />
    protected override MovingNotification<IMediaType> GetMovingNotification(
        MoveEventInfo<IMediaType> moveInfo,
        EventMessages eventMessages) => new MediaTypeMovingNotification(moveInfo, eventMessages);

    /// <inheritdoc />
    protected override MovedNotification<IMediaType> GetMovedNotification(
        IEnumerable<MoveEventInfo<IMediaType>> moveInfo, EventMessages eventMessages) =>
        new MediaTypeMovedNotification(moveInfo, eventMessages);

    /// <inheritdoc />
    protected override ContentTypeChangeNotification<IMediaType> GetContentTypeChangedNotification(
        IEnumerable<ContentTypeChange<IMediaType>> changes, EventMessages eventMessages) =>
        new MediaTypeChangedNotification(changes, eventMessages);

    /// <inheritdoc />
    protected override ContentTypeRefreshNotification<IMediaType> GetContentTypeRefreshedNotification(
        IEnumerable<ContentTypeChange<IMediaType>> changes, EventMessages eventMessages) =>
        new MediaTypeRefreshedNotification(changes, eventMessages);

    #endregion
}
