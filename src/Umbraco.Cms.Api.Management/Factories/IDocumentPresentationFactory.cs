using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentPresentationFactory
{
    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content);

    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity, string? culture = null);

    DocumentBlueprintResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);
}
