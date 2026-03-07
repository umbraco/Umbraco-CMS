using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Collection;

    /// <summary>
    /// Controller responsible for managing collections of documents identified by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentCollectionController : DocumentCollectionControllerBase
{
    private readonly IContentListViewService _contentListViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDocumentCollectionPresentationFactory _documentCollectionPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.Collection.ByKeyDocumentCollectionController"/> class,
    /// which handles document collection operations by document key.
    /// </summary>
    /// <param name="contentListViewService">Service for retrieving and managing content list views.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    /// <param name="documentCollectionPresentationFactory">Factory for creating document collection presentation models.</param>
    /// <param name="flagProviders">A collection of providers for document collection flags.</param>
    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentCollectionController(
        IContentListViewService contentListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IDocumentCollectionPresentationFactory documentCollectionPresentationFactory,
        FlagProviderCollection flagProviders)
        : base(mapper, flagProviders)
    {
        _contentListViewService = contentListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _documentCollectionPresentationFactory = documentCollectionPresentationFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.Collection.ByKeyDocumentCollectionController"/> class.
    /// </summary>
    /// <param name="contentListViewService">Service for managing content list views.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="mapper">Maps Umbraco objects to API models.</param>
    /// <param name="documentCollectionPresentationFactory">Factory for creating document collection presentation models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
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
            StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
    }

    /// <summary>
    /// Retrieves a paged collection of documents identified by the provided unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (key) of the document collection.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the collection.</param>
    /// <param name="orderBy">The field by which to order the collection. Defaults to <c>"updateDate"</c>.</param>
    /// <param name="orderCulture">An optional culture code to use for ordering.</param>
    /// <param name="orderDirection">The direction to order the collection. Defaults to <see cref="Direction.Ascending"/>.</param>
    /// <param name="filter">An optional filter string to filter the collection.</param>
    /// <param name="skip">The number of items to skip for paging. Defaults to 0.</param>
    /// <param name="take">The number of items to take for paging. Defaults to 100.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged list of <see cref="DocumentCollectionResponseModel"/>.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document collection.")]
    [EndpointDescription("Gets a document collection identified by the provided Id.")]
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
