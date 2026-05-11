using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
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
    [ActivatorUtilitiesConstructor]
    public ItemMediaItemController(
        IEntityService entityService,
        IMediaPresentationFactory mediaPresentationFactory,
        FlagProviderCollection flagProviders)
    {
        _entityService = entityService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _flagProviders = flagProviders;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMediaItemController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the controller.</param>
    /// <param name="mediaPresentationFactory">Factory responsible for creating media presentation models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18")]
    public ItemMediaItemController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : this(entityService, mediaPresentationFactory, StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
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

        IEnumerable<IMediaEntitySlim> media = _entityService
            .GetAll(UmbracoObjectTypes.Media, ids.ToArray())
            .OfType<IMediaEntitySlim>();

        IEnumerable<MediaItemResponseModel> responseModels = media.Select(_mediaPresentationFactory.CreateItemResponseModel);
        await PopulateFlags(responseModels);

        return Ok(responseModels);
    }

    private async Task PopulateFlags(IEnumerable<MediaItemResponseModel> itemViewModels)
    {
        foreach (IFlagProvider signProvider in _flagProviders.Where(x => x.CanProvideFlags<MediaItemResponseModel>()))
        {
            await signProvider.PopulateFlagsAsync(itemViewModels);
        }
    }
}
