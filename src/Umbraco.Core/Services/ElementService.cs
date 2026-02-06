using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Services;

public class ElementService : PublishableContentServiceBase<IElement>, IElementService
{
    private readonly IElementRepository _elementRepository;
    private readonly ILogger<ElementService> _logger;
    private readonly IShortStringHelper _shortStringHelper;

    public ElementService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IElementRepository elementRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IShortStringHelper shortStringHelper)
        : base(
            provider,
            loggerFactory,
            eventMessagesFactory,
            auditService,
            contentTypeRepository,
            elementRepository,
            languageRepository,
            propertyValidationService,
            cultureImpactFactory,
            userIdKeyResolver,
            propertyEditorCollection,
            idKeyMap)
    {
        _elementRepository = elementRepository;
        _shortStringHelper = shortStringHelper;
        _logger = loggerFactory.CreateLogger<ElementService>();
    }

    #region Create

    public IElement Create(string name, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContentType contentType = GetContentType(contentTypeAlias)
                                   // causes rollback
                                   ?? throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

        var element = new Element(name, contentType, userId);

        return element;
    }

    #endregion

    #region Others

    // TODO ELEMENTS: create abstractions of the implementations in this region, and share them with ContentService

    Attempt<OperationResult?> IContentServiceBase<IElement>.Save(IEnumerable<IElement> contents, int userId) =>
        Attempt.Succeed(Save(contents, userId));

    public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            ContentDataIntegrityReport report = _elementRepository.CheckDataIntegrity(options);

            if (report.FixedIssues.Count > 0)
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Element("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ElementTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
            }

            scope.Complete();

            return report;
        }
    }

    #endregion

    #region Content Types

    /// <inheritdoc/>
    public void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        var changes = new List<TreeChange<IElement>>();
        var contentTypeIdsA = contentTypeIds.ToArray();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds);

        IQuery<IElement> query = Query<IElement>().WhereIn(x => x.ContentTypeId, contentTypeIdsA);
        IElement[] elements = _elementRepository.Get(query).ToArray();

        if (elements.Length is 0)
        {
            scope.Complete();
            return;
        }

        if (scope.Notifications.PublishCancelable(new ElementDeletingNotification(elements, eventMessages)))
        {
            scope.Complete();
            return;
        }

        foreach (IElement element in elements)
        {
            // delete content
            // triggers the deleted event
            DeleteLocked(scope, element, eventMessages);
            changes.Add(new TreeChange<IElement>(element, TreeChangeTypes.Remove));
        }

        scope.Notifications.Publish(new ElementTreeChangeNotification(changes, eventMessages));

        Audit(AuditType.Delete, userId, Constants.System.Root, $"Delete element of type {string.Join(",", contentTypeIdsA)}");

        scope.Complete();
    }

    #endregion

    #region Abstract implementations

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.Element;

    protected override int[] ReadLockIds => WriteLockIds;

    protected override int[] WriteLockIds => new[] { Constants.Locks.ElementTree };

    protected override bool SupportsBranchPublishing => false;

    protected override ILogger<ElementService> Logger => _logger;

    protected override IElement CreateContentInstance(string name, int parentId, IContentType contentType, int userId)
        => new Element(name, contentType, userId);

    // TODO ELEMENTS: this should only be on the content service
    protected override IElement CreateContentInstance(string name, IElement parent, IContentType contentType, int userId)
        => throw new InvalidOperationException("Elements cannot be nested underneath one another");

    protected override void DeleteLocked(ICoreScope scope, IElement content, EventMessages evtMsgs)
    {
        _elementRepository.Delete(content);
        scope.Notifications.Publish(new ElementDeletedNotification(content, evtMsgs));
    }

    protected override SavingNotification<IElement> SavingNotification(IElement content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IElement content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override SavingNotification<IElement> SavingNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> SavedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IElement content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IElement content, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, publishedCultures, unpublishedCultures, eventMessages);

    protected override TreeChangeNotification<IElement> TreeChangeNotification(IEnumerable<IElement> content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override DeletingNotification<IElement> DeletingNotification(IElement content, EventMessages eventMessages)
        => new ElementDeletingNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> PublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishingNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override IStatefulNotification PublishedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> UnpublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishingNotification(content, eventMessages);

    protected override IStatefulNotification UnpublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishedNotification(content, eventMessages);

    protected override RollingBackNotification<IElement> RollingBackNotification(IElement target, EventMessages messages)
        => new ElementRollingBackNotification(target, messages);

    protected override RolledBackNotification<IElement> RolledBackNotification(IElement target, EventMessages messages)
        => new ElementRolledBackNotification(target, messages);

    #endregion
}
