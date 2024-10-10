using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

[ApiVersion("1.0")]
public class ByKeyMediaCollectionController : MediaCollectionControllerBase
{
    private readonly IMediaListViewService _mediaListViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ByKeyMediaCollectionController(
        IMediaListViewService mediaListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
        : base(mapper)
    {
        _mediaListViewService = mediaListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(
        CancellationToken cancellationToken,
        Guid? id,
        Guid? dataTypeId = null,
        string orderBy = "updateDate",
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
            orderDirection,
            filter,
            skip,
            take);

        return collectionAttempt.Success
            ? CollectionResult(collectionAttempt.Result!)
            : CollectionOperationStatusResult(collectionAttempt.Status);
    }
}
