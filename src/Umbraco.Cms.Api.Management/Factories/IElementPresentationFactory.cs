using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IElementPresentationFactory
{
    ElementResponseModel CreateResponseModel(IElement element, ContentScheduleCollection schedule);

    ElementItemResponseModel CreateItemResponseModel(IElementEntitySlim entity);

    IEnumerable<ElementVariantItemResponseModel> CreateVariantsItemResponseModels(IElementEntitySlim entity);

    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IElementEntitySlim entity);
}
