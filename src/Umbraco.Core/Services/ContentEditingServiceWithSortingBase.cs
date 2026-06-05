using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for content editing services that support sorting operations.
/// </summary>
/// <typeparam name="TContent">The type of content (e.g., IContent, IMedia).</typeparam>
/// <typeparam name="TContentType">The type of content type.</typeparam>
/// <typeparam name="TContentService">The type of content service.</typeparam>
/// <typeparam name="TContentTypeService">The type of content type service.</typeparam>
internal abstract class ContentEditingServiceWithSortingBase<TContent, TContentType, TContentService, TContentTypeService>
    : ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentService : IContentServiceBase<TContent>
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> _logger;
    private readonly ITreeEntitySortingService _treeEntitySortingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentEditingServiceWithSortingBase{TContent, TContentType, TContentService, TContentTypeService}"/> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="propertyEditorCollection">The property editor collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="validationService">The validation service.</param>
    /// <param name="treeEntitySortingService">The tree entity sorting service.</param>
    /// <param name="optionsMonitor">The content settings options monitor.</param>
    /// <param name="relationService">The relation service.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    protected ContentEditingServiceWithSortingBase(
        TContentService contentService,
        TContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationServiceBase<TContentType> validationService,
        ITreeEntitySortingService treeEntitySortingService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters)
        : base(
            contentService,
            contentTypeService,
            propertyEditorCollection,
            dataTypeService,
            logger,
            scopeProvider,
            userIdKeyResolver,
            validationService,
            optionsMonitor,
            relationService,
            contentTypeFilters)
    {
        _logger = logger;
        _treeEntitySortingService = treeEntitySortingService;
    }

    /// <summary>
    /// Sorts the specified items.
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="userId">The user performing the sort operation.</param>
    /// <returns>The operation status.</returns>
    protected abstract ContentEditingOperationStatus Sort(IEnumerable<TContent> items, int userId);

    /// <summary>
    /// Gets the paged children of the specified parent.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> to use the default (sort order).</param>
    /// <param name="total">The total number of children.</param>
    /// <returns>The paged children.</returns>
    protected abstract IEnumerable<TContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, Ordering? ordering, out long total);

    /// <summary>
    /// Handles the sorting operation asynchronously.
    /// </summary>
    /// <param name="parentKey">The optional parent key.</param>
    /// <param name="sortingModels">The sorting models.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <returns>The operation status.</returns>
    protected async Task<ContentEditingOperationStatus> HandleSortAsync(
        Guid? parentKey,
        IEnumerable<SortingModel> sortingModels,
        Guid userKey)
    {
        var contentId = parentKey.HasValue
            ? ContentService.GetById(parentKey.Value)?.Id
            : Constants.System.Root;

        if (contentId.HasValue is false)
        {
            return ContentEditingOperationStatus.NotFound;
        }

        List<TContent> children = LoadAllChildren(contentId.Value, ordering: null);

        try
        {
            TContent[] sortedChildren = _treeEntitySortingService
                .SortEntities(children, sortingModels)
                .ToArray();

            var userId = await GetUserIdAsync(userKey);

            return Sort(sortedChildren, userId);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogError(argumentException, "Invalid sorting instructions, see exception for details.");
            return ContentEditingOperationStatus.SortingInvalid;
        }
    }

    /// <summary>
    /// Handles sorting a parent's children by a system field asynchronously.
    /// </summary>
    /// <param name="parentKey">The optional parent key.</param>
    /// <param name="field">The system field to sort the children by.</param>
    /// <param name="direction">The direction to sort in.</param>
    /// <param name="culture">The culture whose variant name to sort by, or <c>null</c> to sort by the invariant name. Only applies when sorting by <see cref="ContentSortField.Name"/>. The culture is not validated: a child that does not vary by the given culture - or an unrecognised culture - falls back to the invariant name.</param>
    /// <param name="userKey">The user key performing the operation.</param>
    /// <returns>The operation status.</returns>
    protected async Task<ContentEditingOperationStatus> HandleSortByFieldAsync(
        Guid? parentKey,
        ContentSortField field,
        Direction direction,
        string? culture,
        Guid userKey)
    {
        var contentId = parentKey.HasValue
            ? ContentService.GetById(parentKey.Value)?.Id
            : Constants.System.Root;

        if (contentId.HasValue is false)
        {
            return ContentEditingOperationStatus.NotFound;
        }

        Ordering? ordering = BuildOrdering(field, direction, culture);
        if (ordering is null)
        {
            return ContentEditingOperationStatus.SortingInvalid;
        }

        // The database does the ordering (matching the list view and the order shown in the sort UI).
        if (ContentSettings.SortChildrenByFieldFiresNotifications)
        {
            // Opt-in path: load the children and persist via the standard sort, firing per-item
            // save/sort notifications (and therefore webhooks), at the cost of loading every child.
            List<TContent> orderedChildren = LoadAllChildren(contentId.Value, ordering);
            if (orderedChildren.Count == 0)
            {
                return ContentEditingOperationStatus.Success;
            }

            return Sort(orderedChildren, await GetUserIdAsync(userKey));
        }

        // Default path: persist the resulting order with a single set-based update and a branch cache
        // refresh, without loading every child or firing per-item notifications.
        List<int> orderedChildIds = LoadOrderedChildIds(contentId.Value, ordering);
        if (orderedChildIds.Count == 0)
        {
            // Nothing to sort - the order is trivially correct.
            return ContentEditingOperationStatus.Success;
        }

        return SortChildrenInBulk(contentId.Value, orderedChildIds, await GetUserIdAsync(userKey));
    }

    /// <summary>
    /// Persists the supplied (already ordered) child identifiers as the new sort order, without loading
    /// the children or firing per-item notifications.
    /// </summary>
    /// <param name="parentId">The parent identifier, or the root identifier for root-level sorting.</param>
    /// <param name="orderedChildIds">The child identifiers in their desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation status.</returns>
    protected abstract ContentEditingOperationStatus SortChildrenInBulk(int parentId, IReadOnlyList<int> orderedChildIds, int userId);

    private List<int> LoadOrderedChildIds(int contentId, Ordering ordering)
        => LoadAllChildren(contentId, ordering, child => child.Id);

    private List<TContent> LoadAllChildren(int contentId, Ordering? ordering)
        => LoadAllChildren(contentId, ordering, child => child);

    // Pages through all children, projecting each page with the selector so callers that only need a
    // lightweight value (e.g. the id) don't retain every loaded child.
    private List<TResult> LoadAllChildren<TResult>(int contentId, Ordering? ordering, Func<TContent, TResult> selector)
    {
        const int pageSize = 500;
        var pageNumber = 0;
        IEnumerable<TContent> page = GetPagedChildren(contentId, pageNumber++, pageSize, ordering, out var total);
        var results = new List<TResult>((int)total);
        results.AddRange(page.Select(selector));
        while (pageNumber * pageSize < total)
        {
            page = GetPagedChildren(contentId, pageNumber++, pageSize, ordering, out _);
            results.AddRange(page.Select(selector));
        }

        return results;
    }

    private static Ordering? BuildOrdering(ContentSortField field, Direction direction, string? culture)
        => field switch
        {
            // Name is variant - the culture selects the variant name to order by (invariant content and media
            // ignore it). Create and update dates are node-level, so the culture does not apply.
            ContentSortField.Name => Ordering.By("name", direction, culture),
            ContentSortField.CreateDate => Ordering.By("createDate", direction),
            ContentSortField.UpdateDate => Ordering.By("updateDate", direction),
            _ => null,
        };
}
