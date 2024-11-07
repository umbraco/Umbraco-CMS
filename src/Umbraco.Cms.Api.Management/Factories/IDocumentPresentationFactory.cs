using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentPresentationFactory
{
    [Obsolete("Schedule for removal in v17")]
    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content);

    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule)
#pragma warning disable CS0618 // Type or member is obsolete
        // Remove when obsolete CreateResponseModelAsync is removed
        => CreateResponseModelAsync(content);
#pragma warning restore CS0618 // Type or member is obsolete

    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity);

    DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);

    IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity);

    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity);

    Attempt<CultureAndScheduleModel, ContentPublishingOperationStatus> CreateCultureAndScheduleModel(PublishDocumentRequestModel requestModel);
}
