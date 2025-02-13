using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IPublicAccessService _publicAccessService;

    [Obsolete("Please use the constructor taking all parameters. This constructor will be removed in Umbraco 16.")]
    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper)
        : this(
              contentListViewService,
              backOfficeSecurityAccessor,
              mapper,
              StaticServiceProvider.Instance.GetRequiredService<IPublicAccessService>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IPublicAccessService publicAccessService)
        : base(mapper)
    {
        _contentListViewService = contentListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _publicAccessService = publicAccessService;
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

        if (collectionAttempt.Success is false)
        {
            return CollectionOperationStatusResult(collectionAttempt.Status);
        }

        IActionResult actionResult = CollectionResult(collectionAttempt.Result!);

        PopulateIsProtectedField(actionResult, collectionAttempt.Result!);

        return actionResult;
    }

    private void PopulateIsProtectedField(IActionResult actionResult, ListViewPagedModel<IContent> contentCollection)
    {
        // Unpack the result in order to populate the IsProtected property.
        if (actionResult is not OkObjectResult okObjectResult)
        {
            return;
        }

        var typedResult = okObjectResult.Value as PagedViewModel<DocumentCollectionResponseModel>;
        if (typedResult is not null)
        {
            foreach (DocumentCollectionResponseModel item in typedResult.Items)
            {
                IContent? matchingContentItem = contentCollection.Items.Items.FirstOrDefault(x => x.Key == item.Id);
                if (matchingContentItem is null)
                {
                    continue;
                }

                item.IsProtected = _publicAccessService.IsProtected(matchingContentItem).Success;
            }
        }
    }
}
