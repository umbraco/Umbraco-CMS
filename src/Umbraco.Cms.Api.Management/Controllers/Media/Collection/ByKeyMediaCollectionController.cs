using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

/// <summary>
/// Provides API endpoints for managing media collections by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyMediaCollectionController : MediaCollectionControllerBase
{
    private readonly IMediaListViewService _mediaListViewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaCollectionPresentationFactory _mediaCollectionPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Media.Collection.ByKeyMediaCollectionController"/> class.
    /// </summary>
    /// <param name="mediaListViewService">Service for handling media list views.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mapper">The Umbraco object mapper.</param>
    /// <param name="mediaCollectionPresentationFactory">Factory for creating media collection presentation models.</param>
    /// <param name="flagProviders">A collection of flag providers for the media collection.</param>
    [ActivatorUtilitiesConstructor]
    public ByKeyMediaCollectionController(
        IMediaListViewService mediaListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IMediaCollectionPresentationFactory mediaCollectionPresentationFactory,
        FlagProviderCollection flagProviders)
        : base(mapper, flagProviders)
    {
        _mediaListViewService = mediaListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaCollectionPresentationFactory = mediaCollectionPresentationFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMediaCollectionController"/> class with the specified services and factories required for media collection operations.
    /// </summary>
    /// <param name="mediaListViewService">Service for handling media list view operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mapper">The Umbraco object mapper.</param>
    /// <param name="mediaCollectionPresentationFactory">Factory for creating media collection presentation models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled to be removed in Umbraco 18")]
    public ByKeyMediaCollectionController(
        IMediaListViewService mediaListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IMediaCollectionPresentationFactory mediaCollectionPresentationFactory)
        : this(
            mediaListViewService,
            backOfficeSecurityAccessor,
            mapper,
            mediaCollectionPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of media items for the specified collection key, with optional filtering and sorting.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique key identifying the media collection to retrieve items from.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the media items.</param>
    /// <param name="orderBy">The field by which to order the results. Defaults to "updateDate".</param>
    /// <param name="orderDirection">The direction in which to order the results. Defaults to ascending.</param>
    /// <param name="filter">An optional string to filter the media items.</param>
    /// <param name="skip">The number of items to skip for pagination. Defaults to 0.</param>
    /// <param name="take">The maximum number of items to return. Defaults to 100.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paginated collection of <see cref="MediaCollectionResponseModel"/> items, or an error response if the request is invalid or the collection is not found.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a collection of media items.")]
    [EndpointDescription("Gets a paginated collection of media items, optionally filtered and sorted.")]
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


        if (collectionAttempt.Success is false)
        {
            return CollectionOperationStatusResult(collectionAttempt.Status);
        }

        List<MediaCollectionResponseModel> collectionResponseModels = await _mediaCollectionPresentationFactory.CreateCollectionModelAsync(collectionAttempt.Result!);
        return CollectionResult(collectionResponseModels, collectionAttempt.Result!.Items.Total);
    }
}
