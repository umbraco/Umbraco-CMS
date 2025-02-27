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

public class MediaTypeService : ContentTypeServiceBase<IMediaTypeRepository, IMediaType>, IMediaTypeService
{
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
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaTypeRepository,
            auditRepository,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters) => MediaService = mediaService;

    [Obsolete("Use the constructor with all dependencies instead")]
    public MediaTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IMediaService mediaService,
        IMediaTypeRepository mediaTypeRepository,
        IAuditRepository auditRepository,
        IMediaTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaService,
            mediaTypeRepository,
            auditRepository,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>())
    {
    }

    [Obsolete("Use the constructor with all dependencies instead")]
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
        IUserIdKeyResolver userIdKeyResolver)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            mediaService,
            mediaTypeRepository,
            auditRepository,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            StaticServiceProvider.Instance.GetRequiredService<ContentTypeFilterCollection>())
    {
    }

    protected override int[] ReadLockIds => MediaTypeLocks.ReadLockIds;

    protected override int[] WriteLockIds => MediaTypeLocks.WriteLockIds;

    protected override Guid ContainedObjectType => Constants.ObjectTypes.MediaType;

    private IMediaService MediaService { get; }

    protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
    {
        foreach (var typeId in typeIds)
        {
            MediaService.DeleteMediaOfType(typeId);
        }
    }

    protected override bool CanDelete(IMediaType item)
        => item.IsSystemMediaType() is false;

    #region Notifications

    protected override SavingNotification<IMediaType> GetSavingNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeSavingNotification(item, eventMessages);

    protected override SavingNotification<IMediaType> GetSavingNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeSavingNotification(items, eventMessages);

    protected override SavedNotification<IMediaType> GetSavedNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeSavedNotification(item, eventMessages);

    protected override SavedNotification<IMediaType> GetSavedNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeSavedNotification(items, eventMessages);

    protected override DeletingNotification<IMediaType> GetDeletingNotification(
        IMediaType item,
        EventMessages eventMessages) => new MediaTypeDeletingNotification(item, eventMessages);

    protected override DeletingNotification<IMediaType> GetDeletingNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeDeletingNotification(items, eventMessages);

    protected override DeletedNotification<IMediaType> GetDeletedNotification(
        IEnumerable<IMediaType> items,
        EventMessages eventMessages) => new MediaTypeDeletedNotification(items, eventMessages);

    protected override MovingNotification<IMediaType> GetMovingNotification(
        MoveEventInfo<IMediaType> moveInfo,
        EventMessages eventMessages) => new MediaTypeMovingNotification(moveInfo, eventMessages);

    protected override MovedNotification<IMediaType> GetMovedNotification(
        IEnumerable<MoveEventInfo<IMediaType>> moveInfo, EventMessages eventMessages) =>
        new MediaTypeMovedNotification(moveInfo, eventMessages);

    protected override ContentTypeChangeNotification<IMediaType> GetContentTypeChangedNotification(
        IEnumerable<ContentTypeChange<IMediaType>> changes, EventMessages eventMessages) =>
        new MediaTypeChangedNotification(changes, eventMessages);

    protected override ContentTypeRefreshNotification<IMediaType> GetContentTypeRefreshedNotification(
        IEnumerable<ContentTypeChange<IMediaType>> changes, EventMessages eventMessages) =>
        new MediaTypeRefreshedNotification(changes, eventMessages);

    #endregion
}
