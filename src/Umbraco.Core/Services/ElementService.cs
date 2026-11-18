using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages elements.
/// </summary>
public class ElementService : AsyncPublishableContentServiceBase<IElement>, IElementService
{
    private readonly IAsyncElementRepository _elementRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ILogger<ElementService> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="contentTypeRepository">The content type repository.</param>
    /// <param name="elementRepository">The element repository.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="propertyValidationService">The property validation service.</param>
    /// <param name="cultureImpactFactory">The culture impact factory.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="propertyEditorCollection">The property editor collection.</param>
    /// <param name="idKeyMap">The ID key map.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="userService">The user service, resolved lazily to avoid a circular dependency.</param>
    public ElementService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IContentTypeRepository contentTypeRepository,
        IAsyncElementRepository elementRepository,
        ILanguageRepository languageRepository,
        Lazy<IPropertyValidationService> propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        IUserIdKeyResolver userIdKeyResolver,
        PropertyEditorCollection propertyEditorCollection,
        IIdKeyMap idKeyMap,
        IShortStringHelper shortStringHelper,
        Lazy<IUserService> userService)
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
            idKeyMap,
            userService)
    {
        _elementRepository = elementRepository;
        _contentTypeRepository = contentTypeRepository;
        _shortStringHelper = shortStringHelper;
        _logger = loggerFactory.CreateLogger<ElementService>();
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public override Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken)
        => CheckDataIntegrityAsync(
            options,
            scope =>
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Element("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ElementTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
                return Task.CompletedTask;
            },
            cancellationToken);

    /// <inheritdoc />
    public override async Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> contentTypeKeys, Guid userKey, CancellationToken cancellationToken)
    {
        var changes = new List<TreeChange<IElement>>();
        Guid[] contentTypeKeysArray = [.. contentTypeKeys];
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds);

        foreach (Guid contentTypeKey in contentTypeKeysArray)
        {
            if (await _contentTypeRepository.ExistsAsync(contentTypeKey, cancellationToken) is false)
            {
                scope.Complete();
                return Attempt.Fail(ContentDeleteOfTypesOperationStatus.NotFound);
            }
        }

        var elements = new List<IElement>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IElement> elementsPage = await GetPagedOfTypesAsync(contentTypeKeysArray, page++ * pageSize, pageSize, ordering: null, cancellationToken);
            elements.AddRange(elementsPage.Items);
            total = elementsPage.Total;
        }

        if (elements.Count is 0)
        {
            scope.Complete();
            return Attempt.Succeed(ContentDeleteOfTypesOperationStatus.Success);
        }

        if (await scope.Notifications.PublishCancelableAsync(new ElementDeletingNotification(elements, eventMessages)))
        {
            scope.Complete();
            return Attempt.Fail(ContentDeleteOfTypesOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        foreach (IElement element in elements)
        {
            // An element being deleted while published is reported as unpublished; there is nothing to cancel or save.
            if (element is { Trashed: false, Published: true })
            {
                scope.Notifications.Publish(new ElementUnpublishedNotification(
                    element,
                    eventMessages,
                    BuildCultureMap(element, element.ContentType.VariesByCulture() ? element.PublishedCultures : ["*"])));
            }

            await DeleteLockedAsync(scope, element, eventMessages, cancellationToken);
            changes.Add(new TreeChange<IElement>(element, TreeChangeTypes.Remove));
        }

        scope.Notifications.Publish(new ElementTreeChangeNotification(changes, eventMessages));

        await AuditAsync(AuditType.Delete, userId, Constants.System.Root, $"Delete element of type {string.Join(",", contentTypeKeysArray)}");

        scope.Complete();
        return Attempt.Succeed(ContentDeleteOfTypesOperationStatus.Success);
    }

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.Element;

    protected override int RecycleBinId => Constants.System.RecycleBinElement;

    protected override int[] ReadLockIds => WriteLockIds;

    protected override int[] WriteLockIds => [Constants.Locks.ElementTree];

    protected override bool SupportsBranchPublishing => false;

    protected override ILogger<ElementService> Logger => _logger;

    /// <inheritdoc cref="AsyncPublishableContentServiceBase{TContent}.DeleteLockedAsync" />
    protected override async Task DeleteLockedAsync(ICoreScope scope, IElement content, EventMessages evtMsgs, CancellationToken cancellationToken)
    {
        await _elementRepository.DeleteAsync(content, cancellationToken);
        scope.Notifications.Publish(new ElementDeletedNotification(content, evtMsgs));
    }

    protected override SavingNotification<IElement> CreateSavingNotification(IElement content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> CreateSavedNotification(IElement content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override SavedNotification<IElement> CreateSavedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ElementSavedNotification(content, eventMessages, savedCultures);

    protected override SavingNotification<IElement> CreateSavingNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavingNotification(content, eventMessages);

    protected override SavedNotification<IElement> CreateSavedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementSavedNotification(content, eventMessages);

    protected override SavedNotification<IElement> CreateSavedNotification(IEnumerable<IElement> content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? savedCultures)
        => new ElementSavedNotification(content, eventMessages, savedCultures);

    protected override TreeChangeNotification<IElement> CreateTreeChangeNotification(IElement content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override TreeChangeNotification<IElement> CreateTreeChangeNotification(IElement content, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, publishedCultures, unpublishedCultures, eventMessages);

    protected override TreeChangeNotification<IElement> CreateTreeChangeNotification(IEnumerable<IElement> content, TreeChangeTypes changeTypes, EventMessages eventMessages)
        => new ElementTreeChangeNotification(content, changeTypes, eventMessages);

    protected override DeletingNotification<IElement> CreateDeletingNotification(IElement content, EventMessages eventMessages)
        => new ElementDeletingNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> CreatePublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishingNotification(content, eventMessages);

    protected override IStatefulNotification CreatePublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override IStatefulNotification CreatePublishedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? publishedCultures, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ElementPublishedNotification(content, eventMessages, publishedCultures, unpublishedCultures);

    protected override IStatefulNotification CreatePublishedNotification(IEnumerable<IElement> content, EventMessages eventMessages)
        => new ElementPublishedNotification(content, eventMessages);

    protected override CancelableEnumerableObjectNotification<IElement> CreateUnpublishingNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishingNotification(content, eventMessages);

    protected override IStatefulNotification CreateUnpublishedNotification(IElement content, EventMessages eventMessages)
        => new ElementUnpublishedNotification(content, eventMessages);

    protected override IStatefulNotification CreateUnpublishedNotification(IElement content, EventMessages eventMessages, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>>? unpublishedCultures)
        => new ElementUnpublishedNotification(content, eventMessages, unpublishedCultures);

    protected override RollingBackNotification<IElement> CreateRollingBackNotification(IElement target, EventMessages messages)
        => new ElementRollingBackNotification(target, messages);

    protected override RolledBackNotification<IElement> CreateRolledBackNotification(IElement target, EventMessages messages)
        => new ElementRolledBackNotification(target, messages);

    protected override DeletingVersionsNotification<IElement> CreateDeletingVersionsNotification(Guid key, EventMessages eventMessages, int specificVersion, DateTime dateToRetain)
        => new ElementDeletingVersionsNotification(key, eventMessages, specificVersion, dateToRetain: dateToRetain);

    protected override DeletedVersionsNotification<IElement> CreateDeletedVersionsNotification(Guid key, EventMessages eventMessages, int specificVersion, DateTime dateToRetain)
        => new ElementDeletedVersionsNotification(key, eventMessages, specificVersion, dateToRetain: dateToRetain);
}
