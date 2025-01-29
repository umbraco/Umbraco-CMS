using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IObjectTypePresentationFactory
{
    IEnumerable<ObjectTypeResponseModel> Create();
}
