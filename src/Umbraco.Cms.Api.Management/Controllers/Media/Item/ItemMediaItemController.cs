using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Item;

[ApiVersion("1.0")]
public class ItemMediaItemController : MediaItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly SignProviderCollection _signProviders;

    [ActivatorUtilitiesConstructor]
    public ItemMediaItemController(
        IEntityService entityService,
        IMediaPresentationFactory mediaPresentationFactory,
        SignProviderCollection signProvider)
    {
        _entityService = entityService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _signProviders = signProvider;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18")]
    public ItemMediaItemController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : this(entityService, mediaPresentationFactory, StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaItemResponseModel>), StatusCodes.Status200OK)]
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
        await PopulateSigns(responseModels);

        return Ok(responseModels);
    }

    private async Task PopulateSigns(IEnumerable<MediaItemResponseModel> itemViewModels)
    {
        foreach (ISignProvider signProvider in _signProviders.Where(x => x.CanProvideSigns<DocumentItemResponseModel>()))
        {
            await signProvider.PopulateSignsAsync(itemViewModels);
        }
    }
}
