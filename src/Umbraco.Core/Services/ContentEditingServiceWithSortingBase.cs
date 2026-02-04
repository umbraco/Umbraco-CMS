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
        ContentTypeFilterCollection contentTypeFilters,
        ILanguageService languageService,
        IUserService userService,
        ILocalizationService localizationService)
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
            userService,
            localizationService)
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
    /// <param name="total">The total number of children.</param>
    /// <returns>The paged children.</returns>
    protected abstract IEnumerable<TContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total);

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

        const int pageSize = 500;
        var pageNumber = 0;
        IEnumerable<TContent> page = GetPagedChildren(contentId.Value, pageNumber++, pageSize, out var total);
        var children = new List<TContent>((int)total);
        children.AddRange(page);
        while (pageNumber * pageSize < total)
        {
            page = GetPagedChildren(contentId.Value, pageNumber++, pageSize, out _);
            children.AddRange(page);
        }

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
}
