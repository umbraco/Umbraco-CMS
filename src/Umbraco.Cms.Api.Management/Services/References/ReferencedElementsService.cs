using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services.References;

/// <inheritdoc />
internal sealed class ReferencedElementsService : IReferencedElementsService
{
    // The two relation type aliases that can link a document/element to an Element: a plain Element Picker
    // property ("umbElement") and an element embedded as reusable block content ("umbExternalBlockElement").
    private static readonly string[] ElementRelationTypeAliases =
    [
        Constants.Conventions.RelationTypes.RelatedElementAlias,
        Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias,
    ];

    private readonly IEntityService _entityService;
    private readonly IRelationService _relationService;
    private readonly IElementService _elementService;
    private readonly TimeProvider _timeProvider;

    public ReferencedElementsService(
        IEntityService entityService,
        IRelationService relationService,
        IElementService elementService,
        TimeProvider timeProvider)
    {
        _entityService = entityService;
        _relationService = relationService;
        _elementService = elementService;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public Task<Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus>> GetPagedReferencedElementsWithPendingChangesAsync(
        Guid parentKey,
        UmbracoObjectTypes parentObjectType,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        IEntitySlim? parent = _entityService.Get(parentKey, parentObjectType);
        if (parent is null)
        {
            return Task.FromResult(Attempt.FailWithStatus(
                GetReferencesOperationStatus.ContentNotFound,
                new PagedModel<ReferencedElementWithPendingChanges>()));
        }

        IEnumerable<IUmbracoEntity> children = _relationService.GetChildEntitiesByParentId(
            parent.Id, ElementRelationTypeAliases, UmbracoObjectTypes.Element);

        ReferencedElementWithPendingChanges[] withPendingChanges = children
            .OfType<IElementEntitySlim>()
            .Select(element => new
            {
                Element = element,
                State = PublishableVariantStateHelper.GetAggregateState(element),
            })
            .Where(x => x.State is PublishableVariantState.Draft or PublishableVariantState.PublishedPendingChanges)
            .OrderBy(x => x.Element.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Element.Key)
            .Select(x => new ReferencedElementWithPendingChanges
            {
                Element = x.Element,
                State = x.State,
                IsScheduled = false, // resolved below, for the current page only
            })
            .ToArray();

        var total = withPendingChanges.Length;
        ReferencedElementWithPendingChanges[] page = withPendingChanges.Skip(skip).Take(take).ToArray();

        if (page.Length > 0)
        {
            page = ResolveScheduled(page);
        }

        return Task.FromResult(Attempt.SucceedWithStatus(
            GetReferencesOperationStatus.Success,
            new PagedModel<ReferencedElementWithPendingChanges>(total, page)));
    }

    private ReferencedElementWithPendingChanges[] ResolveScheduled(ReferencedElementWithPendingChanges[] page)
    {
        Guid[] keys = page.Select(x => x.Element.Key).ToArray();
        IDictionary<Guid, IEnumerable<ContentSchedule>> schedulesByKey = _elementService.GetContentSchedulesByKeys(keys);
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

        return page
            .Select(x =>
            {
                var isScheduled = schedulesByKey.TryGetValue(x.Element.Key, out IEnumerable<ContentSchedule>? schedules)
                    && schedules.Any(s => s.Action == ContentScheduleAction.Release && s.Date > now);

                return isScheduled
                    ? new ReferencedElementWithPendingChanges { Element = x.Element, State = x.State, IsScheduled = true }
                    : x;
            })
            .ToArray();
    }
}
