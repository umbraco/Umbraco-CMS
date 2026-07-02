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
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class ElementService : PublishableContentServiceBase<IElement>, IElementService
{
    private readonly IElementRepository _elementRepository;
    private readonly ILogger<ElementService> _logger;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly PropertyEditorCollection _propertyEditorCollection;

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
        _languageRepository = languageRepository;
        _cultureImpactFactory = cultureImpactFactory;
        _propertyEditorCollection = propertyEditorCollection;
    }

    #region Save, Publish, Unpublish

    /// <inheritdoc />
    public PublishResult SaveAndPublish(IElement content, string[] culturesToPublish, int userId = Constants.Security.SuperUserId)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (culturesToPublish == null)
        {
            throw new ArgumentNullException(nameof(culturesToPublish));
        }

        // wildcards and nulls are not accepted here; cultures must be explicit
        if (culturesToPublish.Any(x => x == null || x == "*"))
        {
            throw new InvalidOperationException(
                "Only valid cultures are allowed to be used in this method, wildcards or nulls are not allowed");
        }

        EnsureCulturesAreValid(culturesToPublish, nameof(culturesToPublish));

        culturesToPublish = culturesToPublish.Select(x => x.EnsureCultureCode()!).ToArray();

        EnsureNameLengthIsValid(content);

        EnsurePublishedStateAllowsPublish(content);

        var varies = content.ContentType.VariesByCulture();
        if (varies is false)
        {
            if (culturesToPublish.Length > 0)
            {
                throw new ArgumentException(
                    "Cultures cannot be specified when publishing invariant content types.",
                    nameof(culturesToPublish));
            }

            // doesn't vary; publish the invariant culture in a single scope alongside the save
            return SaveAndPublish(content, userId: userId);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds);

        var allLangs = _languageRepository.GetMany().ToList();

        EventMessages evtMsgs = EventMessagesFactory.Get();

        SavingNotification<IElement> savingNotification = SavingNotification(content, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
        }

        IEnumerable<CultureImpact> impacts =
            culturesToPublish.Select(x => _cultureImpactFactory.ImpactExplicit(x, IsDefaultCulture(allLangs, x)));

        // publish the culture(s)
        // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
        foreach (CultureImpact impact in impacts)
        {
            content.PublishCulture(impact, DateTime.UtcNow, _propertyEditorCollection);
        }

        PublishResult result = CommitContentChangesInternal(scope, content, evtMsgs, allLangs, savingNotification.State, userId);
        scope.Complete();
        return result;
    }

    private PublishResult SaveAndPublish(IElement content, string culture = "*", int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        EnsurePublishedStateAllowsPublish(content);

        // cannot accept invariant (null or empty) culture for variant content type
        // cannot accept a specific culture for invariant content type (but '*' is ok)
        if (content.ContentType.VariesByCulture())
        {
            if (culture.IsNullOrWhiteSpace())
            {
                throw new NotSupportedException("Invariant culture is not supported by variant content types.");
            }
        }
        else
        {
            if (!culture.IsNullOrWhiteSpace() && culture != "*")
            {
                throw new NotSupportedException(
                    $"Culture \"{culture}\" is not supported by invariant content types.");
            }
        }

        EnsureNameLengthIsValid(content);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds);

        var allLangs = _languageRepository.GetMany().ToList();

        // Change state to publishing
        content.PublishedState = PublishedState.Publishing;
        SavingNotification<IElement> savingNotification = SavingNotification(content, evtMsgs);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            return new PublishResult(PublishResultType.FailedPublishCancelledByEvent, evtMsgs, content);
        }

        // this will create the correct culture impact even if culture is * or null
        var impact = _cultureImpactFactory.Create(culture, IsDefaultCulture(allLangs, culture), content);

        // publish the culture(s)
        // we don't care about the response here, this response will be rechecked below but we need to set the culture info values now.
        content.PublishCulture(impact, DateTime.UtcNow, _propertyEditorCollection);

        PublishResult result = CommitContentChangesInternal(scope, content, evtMsgs, allLangs, savingNotification.State, userId);
        scope.Complete();
        return result;
    }

    private const int MaxContentNameLength = 255;

    private static void EnsureNameLengthIsValid(IElement content)
    {
        if (content.Name?.Length > MaxContentNameLength)
        {
            throw new InvalidOperationException($"Name cannot be more than {MaxContentNameLength} characters in length.");
        }
    }

    private static void EnsureCulturesAreValid(string[] cultures, string paramName)
    {
        if (cultures.Any(c => c.IsNullOrWhiteSpace()) || cultures.Distinct().Count() != cultures.Length)
        {
            throw new ArgumentException("Cultures cannot be null or whitespace, and must be distinct.", paramName);
        }
    }

    private static void EnsurePublishedStateAllowsPublish(IElement content)
    {
        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                "Cannot save-and-publish (un)publishing content, use the dedicated commit method.");
        }
    }

    #endregion

    #region Others

    /// <inheritdoc />
    public override ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
        => CheckDataIntegrity(
            options,
            scope =>
            {
                // The event args needs a content item so we'll make a fake one with enough properties to not cause a null ref
                var root = new Element("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
                scope.Notifications.Publish(new ElementTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
            });

    #endregion

    #region Content Types

    /// <inheritdoc/>
    public override void DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
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
