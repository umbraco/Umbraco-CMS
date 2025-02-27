using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Collection;

[ApiVersion("1.0")]
public class ByKeyDocumentCollectionController : DocumentCollectionControllerBase
{
    private readonly IContentListViewService _contentListViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
        : base(mapper)
    {
        _contentListViewService = contentListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(
        CancellationToken cancellationToken,
        Guid id,
        Guid? dataTypeId = null,
        string orderBy = "updateDate",
        string? orderCulture = null,
        Direction orderDirection = Direction.Ascending,
        string? filter = null,
        int skip = 0,
        int take = 100)
    {
        Attempt<ListViewPagedModel<IContent>?, ContentCollectionOperationStatus> collectionAttempt = await _contentListViewService.GetListViewItemsByKeyAsync(
            CurrentUser(_backOfficeSecurityAccessor),
            id,
            dataTypeId,
            orderBy,
            orderCulture,
            orderDirection,
            filter,
            skip,
            take);

        return collectionAttempt.Success
            ? CollectionResult(collectionAttempt.Result!)
            : CollectionOperationStatusResult(collectionAttempt.Status);
    }
}
