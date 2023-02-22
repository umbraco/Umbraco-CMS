using Umbraco.Cms.Api.Management.ViewModels.RelationType;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IObjectTypeViewModelFactory
{
    IEnumerable<ObjectTypeResponseModel> Create();
}
