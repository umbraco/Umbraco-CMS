using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IConfigurationPresentationFactory
{
    DocumentConfigurationResponseModel Create();
}
