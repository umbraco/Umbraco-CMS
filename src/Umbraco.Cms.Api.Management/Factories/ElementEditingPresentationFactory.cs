using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class ElementEditingPresentationFactory : ContentEditingPresentationFactory<ElementValueModel, ElementVariantRequestModel>, IElementEditingPresentationFactory
{
    public ElementCreateModel MapCreateModel(CreateElementRequestModel requestModel)
    {
        ElementCreateModel model = MapContentEditingModel<ElementCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ElementUpdateModel MapUpdateModel(UpdateElementRequestModel requestModel)
        => MapContentEditingModel<ElementUpdateModel>(requestModel);

    public ValidateElementUpdateModel MapValidateUpdateModel(ValidateUpdateElementRequestModel requestModel)
    {
        ValidateElementUpdateModel model = MapContentEditingModel<ValidateElementUpdateModel>(requestModel);
        model.Cultures = requestModel.Cultures;

        return model;
    }
}
