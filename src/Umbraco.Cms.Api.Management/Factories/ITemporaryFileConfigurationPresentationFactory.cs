using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Factories;

public interface ITemporaryFileConfigurationPresentationFactory
{
    TemporaryFileConfigurationResponseModel Create();
}
