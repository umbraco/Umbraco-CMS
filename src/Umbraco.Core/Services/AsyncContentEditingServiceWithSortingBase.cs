// Copyright (c) Umbraco.
// See LICENSE for more details.

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

internal abstract class AsyncContentEditingServiceWithSortingBase<TContent, TContentType, TContentService, TContentTypeService>
    : AsyncContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentService : IAsyncContentServiceBase<TContent>
    where TContentTypeService : IAsyncContentTypeBaseService<TContentType>
{
    private readonly ILogger<AsyncContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> _logger;
    private readonly ITreeEntitySortingService _treeEntitySortingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncContentEditingServiceWithSortingBase{TContent, TContentType, TContentService, TContentTypeService}"/> class.
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
    protected AsyncContentEditingServiceWithSortingBase(
        TContentService contentService,
        TContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        ILogger<AsyncContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> logger,
        ICoreScopeProvider scopeProvider,
        IUserIdKeyResolver userIdKeyResolver,
        IContentValidationServiceBase<TContentType> validationService,
        ITreeEntitySortingService treeEntitySortingService,
        IOptionsMonitor<ContentSettings> optionsMonitor,
        IRelationService relationService,
        ContentTypeFilterCollection contentTypeFilters,
        ILanguageService languageService,
        IUserService userService)
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
            contentTypeFilters,
            languageService,
            userService)
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
    /// <param name="parentKey">The Guid key of the parent, or <c>null</c> for the root of the content tree.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> to use the default (sort order).</param>
    /// <returns>The paged children.</returns>
    protected abstract Task<PagedModel<TContent>> GetPagedChildrenAsync(Guid? parentKey, int pageIndex, int pageSize, Ordering? ordering);

    /// <summary>
    /// Persists the supplied (already ordered) child identifiers as the new sort order, without loading
    /// the children or firing per-item notifications.
    /// </summary>
    /// <param name="parentId">The parent identifier, or the root identifier for root-level sorting.</param>
    /// <param name="orderedChildIds">The child identifiers in their desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation status.</returns>
    protected abstract ContentEditingOperationStatus SortChildrenInBulk(int parentId, IReadOnlyList<int> orderedChildIds, int userId);

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
        if (parentKey.HasValue && await ContentService.GetByIdAsync(parentKey.Value, CancellationToken.None) is null)
        {
            return ContentEditingOperationStatus.NotFound;
        }

        List<TContent> children = await LoadAllChildrenAsync(parentKey, ordering: null);

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
        TContent? parent = parentKey.HasValue
            ? await ContentService.GetByIdAsync(parentKey.Value, CancellationToken.None)
            : null;

        if (parentKey.HasValue && parent is null)
        {
            return ContentEditingOperationStatus.NotFound;
        }

        Ordering ordering = BuildOrdering(field, direction, culture);

        // The database does the ordering (matching the list view and the order shown in the sort UI).
        if (ContentSettings.SortChildrenByFieldFiresNotifications)
        {
            // Opt-in path: load the children and persist via the standard sort, firing per-item
            // save/sort notifications (and therefore webhooks), at the cost of loading every child.
            List<TContent> orderedChildren = await LoadAllChildrenAsync(parentKey, ordering);
            if (orderedChildren.Count == 0)
            {
                return ContentEditingOperationStatus.Success;
            }

            return Sort(orderedChildren, await GetUserIdAsync(userKey));
        }

        // Default path: persist the resulting order with a single set-based update and a branch cache
        // refresh, without loading every child or firing per-item notifications.
        List<int> orderedChildIds = await LoadOrderedChildIdsAsync(parentKey, ordering);
        if (orderedChildIds.Count == 0)
        {
            // Nothing to sort - the order is trivially correct.
            return ContentEditingOperationStatus.Success;
        }

        // SortChildrenInBulk is still int-keyed (it writes through the older IContentService.SortChildren
        // NPoco path) - resolve the parent id here rather than converting that write path too.
        int parentId = parent?.Id ?? Constants.System.Root;
        return SortChildrenInBulk(parentId, orderedChildIds, await GetUserIdAsync(userKey));
    }

    private Task<List<int>> LoadOrderedChildIdsAsync(Guid? parentKey, Ordering ordering)
        => LoadAllChildrenAsync(parentKey, ordering, child => child.Id);

    private Task<List<TContent>> LoadAllChildrenAsync(Guid? parentKey, Ordering? ordering)
        => LoadAllChildrenAsync(parentKey, ordering, child => child);

    // Pages through all children, projecting each page with the selector so callers that only need a
    // lightweight value (e.g. the id) don't retain every loaded child.
    private async Task<List<TResult>> LoadAllChildrenAsync<TResult>(Guid? parentKey, Ordering? ordering, Func<TContent, TResult> selector)
    {
        const int pageSize = 500;
        var pageNumber = 0;
        PagedModel<TContent> page = await GetPagedChildrenAsync(parentKey, pageNumber++, pageSize, ordering);
        var results = new List<TResult>((int)page.Total);
        results.AddRange(page.Items.Select(selector));
        while (pageNumber * pageSize < page.Total)
        {
            page = await GetPagedChildrenAsync(parentKey, pageNumber++, pageSize, ordering);
            results.AddRange(page.Items.Select(selector));
        }

        return results;
    }

    private static Ordering BuildOrdering(ContentSortField field, Direction direction, string? culture)
        => field switch
        {
            // Name is variant - the culture selects the variant name to order by (invariant content and media
            // ignore it). Create and update dates are node-level, so the culture does not apply.
            ContentSortField.Name => Ordering.By("name", direction, culture),
            ContentSortField.CreateDate => Ordering.By("createDate", direction),
            ContentSortField.UpdateDate => Ordering.By("updateDate", direction),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unsupported sort field."),
        };
}

