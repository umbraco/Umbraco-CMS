using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

[ApiVersion("1.0")]
public class ItemDocumentItemController : DocumentItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly SignProviderCollection _signProviders;

    [ActivatorUtilitiesConstructor]
    public ItemDocumentItemController(
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory,
        SignProviderCollection signProvider)
    {
        _entityService = entityService;
        _documentPresentationFactory = documentPresentationFactory;
        _signProviders = signProvider;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18")]
    public ItemDocumentItemController(
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory)
        : this(entityService, documentPresentationFactory, StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<DocumentItemResponseModel>());
        }

        IEnumerable<IDocumentEntitySlim> documents = _entityService
            .GetAll(UmbracoObjectTypes.Document, ids.ToArray())
            .OfType<IDocumentEntitySlim>();

        IEnumerable<DocumentItemResponseModel> responseModels = documents.Select(_documentPresentationFactory.CreateItemResponseModel);
        await PopulateSigns(responseModels);
        return Ok(responseModels);
    }

    private async Task PopulateSigns(IEnumerable<DocumentItemResponseModel> itemViewModels)
    {
        foreach (ISignProvider signProvider in _signProviders.Where(x => x.CanProvideSigns<DocumentItemResponseModel>()))
        {
            await signProvider.PopulateSignsAsync(itemViewModels);
        }
    }
}
