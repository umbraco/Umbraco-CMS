using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentPresentationFactory
{
    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content);

    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity);

    DocumentBlueprintResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);

    IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity);

    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity);
}
