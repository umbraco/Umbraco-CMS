using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IElementEditingPresentationFactory
{
    ElementCreateModel MapCreateModel(CreateElementRequestModel requestModel);

    ElementUpdateModel MapUpdateModel(UpdateElementRequestModel requestModel);

    ValidateElementUpdateModel MapValidateUpdateModel(ValidateUpdateElementRequestModel requestModel);
}
