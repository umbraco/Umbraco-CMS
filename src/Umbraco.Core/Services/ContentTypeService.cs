using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.Locking;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType" />
/// </summary>
public class ContentTypeService : ContentTypeServiceBase<IContentTypeRepository, IContentType>, IContentTypeService
{
    private readonly ITemplateService _templateService;
    private readonly IContentService _contentService;
    private readonly IElementService _elementService;

    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IElementService elementService,
        IContentTypeRepository repository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters,
        ITemplateService templateService)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
        _templateService = templateService;
        _contentService = contentService;
        _elementService = elementService;
    }

    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IContentTypeRepository repository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters,
        ITemplateService templateService)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            StaticServiceProvider.Instance.GetRequiredService<IElementService>(),
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            templateService)
    {
    }

    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IContentTypeRepository repository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            StaticServiceProvider.Instance.GetRequiredService<ITemplateService>())
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IContentTypeRepository repository,
        IAuditRepository auditRepository,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            repository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            StaticServiceProvider.Instance.GetRequiredService<ITemplateService>())
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IContentTypeRepository repository,
        IAuditRepository auditRepository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            StaticServiceProvider.Instance.GetRequiredService<ITemplateService>())
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IContentTypeRepository repository,
        IAuditRepository auditRepository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters,
        ITemplateService templateService)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            templateService)
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ContentTypeService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IContentService contentService,
        IElementService elementService,
        IContentTypeRepository repository,
        IAuditRepository auditRepository,
        IAuditService auditService,
        IDocumentTypeContainerRepository entityContainerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters,
        ITemplateService templateService)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            contentService,
            elementService,
            repository,
            auditService,
            entityContainerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters,
            templateService)
    {
    }

    protected override int[] ReadLockIds => ContentTypeLocks.ReadLockIds;

    protected override int[] WriteLockIds => ContentTypeLocks.WriteLockIds;

    protected override Guid ContainedObjectType => Constants.ObjectTypes.DocumentType;

    /// <summary>
    ///     Gets all property type aliases across content, media and member types.
    /// </summary>
    /// <returns>All property type aliases.</returns>
    /// <remarks>Beware! Works across content, media and member types.</remarks>
    public IEnumerable<string> GetAllPropertyTypeAliases()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            // that one is special because it works across content, media and member types
            scope.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
            return Repository.GetAllPropertyTypeAliases();
        }
    }

    /// <summary>
    ///     Gets all content type aliases across content, media and member types.
    /// </summary>
    /// <param name="guids">Optional object types guid to restrict to content, and/or media, and/or member types.</param>
    /// <returns>All content type aliases.</returns>
    /// <remarks>Beware! Works across content, media and member types.</remarks>
    public IEnumerable<string> GetAllContentTypeAliases(params Guid[] guids)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            // that one is special because it works across content, media and member types
            scope.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
            return Repository.GetAllContentTypeAliases(guids);
        }
    }

    /// <summary>
    ///     Gets all content type id for aliases across content, media and member types.
    /// </summary>
    /// <param name="aliases">Aliases to look for.</param>
    /// <returns>All content type ids.</returns>
    /// <remarks>Beware! Works across content, media and member types.</remarks>
    public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            // that one is special because it works across content, media and member types
            scope.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);
            return Repository.GetAllContentTypeIds(aliases);
        }
    }

    public async Task<IEnumerable<IContentType>> GetByQueryAsync(IQuery<IContentType> query, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        // that one is special because it works across content, media and member types
        scope.ReadLock(Constants.Locks.ContentTypes);
        IEnumerable<IContentType> contentTypes = Repository.Get(query);
        scope.Complete();
        return contentTypes;
    }

    /// <inheritdoc />
    public async Task<Attempt<Guid?, ContentTypeOperationStatus>> CreateTemplateAsync(
        Guid contentTypeKey,
        string templateName,
        string templateAlias,
        bool isDefaultTemplate,
        Guid userKey)
    {
        IContentType? contentType = await GetAsync(contentTypeKey);
        if (contentType is null)
        {
            return Attempt<Guid?, ContentTypeOperationStatus>.Fail(ContentTypeOperationStatus.NotFound);
        }

        Attempt<ITemplate?, TemplateOperationStatus> templateResult =
            await _templateService.CreateForContentTypeAsync(templateName, templateAlias, contentType.Alias, userKey);
        if (templateResult.Success is false)
        {
            return Attempt<Guid?, ContentTypeOperationStatus>.Fail(
                templateResult.Status switch
                {
                    TemplateOperationStatus.CancelledByNotification => ContentTypeOperationStatus
                        .CancelledByNotification,
                    TemplateOperationStatus.InvalidAlias => ContentTypeOperationStatus.InvalidTemplateAlias,
                    _ => ContentTypeOperationStatus.Unknown,
                });
        }

        ITemplate template = templateResult.Result!;
        contentType.AllowedTemplates = [..contentType.AllowedTemplates ?? [], template];
        if (isDefaultTemplate)
        {
            contentType.DefaultTemplateId = template.Id;
        }

        Attempt<ContentTypeOperationStatus> updateContentTypeResult = await UpdateAsync(contentType, userKey);

        return updateContentTypeResult.Success
            ? Attempt<Guid?, ContentTypeOperationStatus>.Succeed(ContentTypeOperationStatus.Success, template.Key)
            : Attempt<Guid?, ContentTypeOperationStatus>.Fail(updateContentTypeResult.Result);
    }

    protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var typeIdsA = typeIds.ToArray();
            _contentService.DeleteOfTypes(typeIdsA);
            _contentService.DeleteBlueprintsOfTypes(typeIdsA);
            _elementService.DeleteOfTypes(typeIdsA);
            scope.Complete();
        }
    }

    #region Notifications

    protected override SavingNotification<IContentType> GetSavingNotification(
        IContentType item,
        EventMessages eventMessages) => new ContentTypeSavingNotification(item, eventMessages);

    protected override SavingNotification<IContentType> GetSavingNotification(
        IEnumerable<IContentType> items,
        EventMessages eventMessages) => new ContentTypeSavingNotification(items, eventMessages);

    protected override SavedNotification<IContentType> GetSavedNotification(
        IContentType item,
        EventMessages eventMessages) => new ContentTypeSavedNotification(item, eventMessages);

    protected override SavedNotification<IContentType> GetSavedNotification(
        IEnumerable<IContentType> items,
        EventMessages eventMessages) => new ContentTypeSavedNotification(items, eventMessages);

    protected override DeletingNotification<IContentType> GetDeletingNotification(
        IContentType item,
        EventMessages eventMessages) => new ContentTypeDeletingNotification(item, eventMessages);

    protected override DeletingNotification<IContentType> GetDeletingNotification(
        IEnumerable<IContentType> items,
        EventMessages eventMessages) => new ContentTypeDeletingNotification(items, eventMessages);

    protected override DeletedNotification<IContentType> GetDeletedNotification(
        IEnumerable<IContentType> items,
        EventMessages eventMessages) => new ContentTypeDeletedNotification(items, eventMessages);

    protected override MovingNotification<IContentType> GetMovingNotification(
        MoveEventInfo<IContentType> moveInfo,
        EventMessages eventMessages) => new ContentTypeMovingNotification(moveInfo, eventMessages);

    protected override MovedNotification<IContentType> GetMovedNotification(
        IEnumerable<MoveEventInfo<IContentType>> moveInfo, EventMessages eventMessages) =>
        new ContentTypeMovedNotification(moveInfo, eventMessages);

    protected override ContentTypeChangeNotification<IContentType> GetContentTypeChangedNotification(
        IEnumerable<ContentTypeChange<IContentType>> changes, EventMessages eventMessages) =>
        new ContentTypeChangedNotification(changes, eventMessages);

    protected override ContentTypeRefreshNotification<IContentType> GetContentTypeRefreshedNotification(
        IEnumerable<ContentTypeChange<IContentType>> changes, EventMessages eventMessages) =>
        new ContentTypeRefreshedNotification(changes, eventMessages);

    #endregion
}
