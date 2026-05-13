using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents a request model used for updating an element via the API.
/// </summary>
public class UpdateElementRequestModel : UpdateContentRequestModelBase<ElementValueModel, ElementVariantRequestModel>
{
}
