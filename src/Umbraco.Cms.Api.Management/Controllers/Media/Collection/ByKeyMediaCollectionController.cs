using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

[ApiVersion("1.0")]
public class ByKeyMediaCollectionController : MediaCollectionControllerBase
{
    private readonly IMediaListViewService _mediaListViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;

    public ByKeyMediaCollectionController(
        IMediaListViewService mediaListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
    {
        _mediaListViewService = mediaListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(
        Guid? id,
        Guid? dataTypeId = null,
        string orderBy = "sortOrder",
        Direction orderDirection = Direction.Ascending,
        string? filter = null,
        int skip = 0,
        int take = 100)
    {
        Attempt<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus> collectionAttempt = await _mediaListViewService.GetListViewItemsByKeyAsync(
            CurrentUser(_backOfficeSecurityAccessor),
            id,
            dataTypeId,
            orderBy,
            null,
            orderDirection,
            filter,
            skip,
            take);

        if (collectionAttempt.Success == false)
        {
            return CollectionOperationStatusResult(collectionAttempt.Status);
        }

        PagedModel<IMedia> collectionItemsResult = collectionAttempt.Result!.Items;
        ListViewConfiguration collectionConfiguration = collectionAttempt.Result!.ListViewConfiguration;

        var collectionPropertyAliases = collectionConfiguration
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        List<MediaCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<IMedia, MediaCollectionResponseModel>(collectionItemsResult.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases);
            });

        var pageViewModel = new PagedViewModel<MediaCollectionResponseModel>
        {
            Total = collectionItemsResult.Total,
            Items = collectionResponseModels
        };

        return Ok(pageViewModel);
    }
}
