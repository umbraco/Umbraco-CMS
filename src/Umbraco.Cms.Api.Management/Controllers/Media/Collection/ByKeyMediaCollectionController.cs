using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IMediaCollectionPresentationFactory _mediaCollectionPresentationFactory;

    [ActivatorUtilitiesConstructor]
    public ByKeyMediaCollectionController(
        IMediaListViewService mediaListViewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IMediaCollectionPresentationFactory mediaCollectionPresentationFactory,
        SignProviderCollection signProviders)
        : base(mapper, signProviders)
    {
        _mediaListViewService = mediaListViewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaCollectionPresentationFactory = mediaCollectionPresentationFactory;
    }

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
            StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
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


        if (collectionAttempt.Success is false)
        {
            return CollectionOperationStatusResult(collectionAttempt.Status);
        }

        List<MediaCollectionResponseModel> collectionResponseModels = await _mediaCollectionPresentationFactory.CreateCollectionModelAsync(collectionAttempt.Result!);
        return CollectionResult(collectionResponseModels, collectionAttempt.Result!.Items.Total);
    }
}
