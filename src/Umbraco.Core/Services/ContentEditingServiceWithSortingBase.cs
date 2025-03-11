using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentEditingServiceWithSortingBase<TContent, TContentType, TContentService, TContentTypeService>
    : ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentService : IContentServiceBase<TContent>
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly ILogger<ContentEditingServiceBase<TContent, TContentType, TContentService, TContentTypeService>> _logger;
    private readonly ITreeEntitySortingService _treeEntitySortingService;

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
        IRelationService relationService)
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
            relationService)
    {
        _logger = logger;
        _treeEntitySortingService = treeEntitySortingService;
    }

    protected abstract ContentEditingOperationStatus Sort(IEnumerable<TContent> items, int userId);

    protected abstract IEnumerable<TContent> GetPagedChildren(int parentId, int pageIndex, int pageSize, out long total);

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
            return await Task.FromResult(ContentEditingOperationStatus.NotFound);
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
