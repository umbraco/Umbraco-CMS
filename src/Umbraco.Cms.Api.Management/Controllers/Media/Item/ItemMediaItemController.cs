using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

/// <summary>
/// Provides API endpoints for managing individual media items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemMediaItemController : MediaItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly FlagProviderCollection _flagProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Media.Item.ItemMediaItemController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the media item controller.</param>
    /// <param name="mediaPresentationFactory">Factory responsible for creating media presentation models.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for media items.</param>
    public ItemMediaItemController(
        IEntityService entityService,
        IMediaPresentationFactory mediaPresentationFactory,
        FlagProviderCollection flagProviders)
    {
        _entityService = entityService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _flagProviders = flagProviders;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media items.")]
    [EndpointDescription("Gets a collection of media items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MediaItemResponseModel>());
        }

        IEnumerable<IEntitySlim> media = _entityService.GetAll(UmbracoObjectTypes.Media, ids.ToArray());

        IEnumerable<MediaItemResponseModel> responseModels = await MapMediaItemsAsync(media);
        await PopulateFlags(responseModels);

        return Ok(responseModels);
    }

    private async Task<IEnumerable<MediaItemResponseModel>> MapMediaItemsAsync(IEnumerable<IEntitySlim> entities)
    {
        List<MediaItemResponseModel> mapped = [];
        foreach (IMediaEntitySlim entity in entities.OfType<IMediaEntitySlim>())
        {
            mapped.Add(await _mediaPresentationFactory.CreateItemResponseModelAsync(entity));
        }

        return mapped;
    }

    private async Task PopulateFlags(IEnumerable<MediaItemResponseModel> itemViewModels)
    {
        foreach (IFlagProvider signProvider in _flagProviders.Where(x => x.CanProvideFlags<MediaItemResponseModel>()))
        {
            await signProvider.PopulateFlagsAsync(itemViewModels);
        }
    }
}
