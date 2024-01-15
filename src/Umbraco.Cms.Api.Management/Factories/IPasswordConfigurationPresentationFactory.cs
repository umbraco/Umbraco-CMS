using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IPasswordConfigurationPresentationFactory
{
    PasswordConfigurationResponseModel CreatePasswordConfigurationResponseModel();
}
