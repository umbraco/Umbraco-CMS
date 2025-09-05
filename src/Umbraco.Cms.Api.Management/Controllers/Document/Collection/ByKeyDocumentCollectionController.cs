using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Signs;
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
    private readonly IDocumentCollectionPresentationFactory _documentCollectionPresentationFactory;

    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IDocumentCollectionPresentationFactory documentCollectionPresentationFactory,
        SignProviderCollection signProviders)
        : base(mapper, signProviders)
    {
        _contentListViewService = contentListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _documentCollectionPresentationFactory = documentCollectionPresentationFactory;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled to be removed in V18")]
    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IDocumentCollectionPresentationFactory documentCollectionPresentationFactory)
        : this(
            contentListViewService,
            backOfficeSecurityAccessor,
            mapper,
            documentCollectionPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
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

        List<DocumentCollectionResponseModel> collectionResponseModels = await _documentCollectionPresentationFactory.CreateCollectionModelAsync(collectionAttempt.Result!);
        return CollectionResult(collectionResponseModels, collectionAttempt.Result!.Items.Total);
    }
}
