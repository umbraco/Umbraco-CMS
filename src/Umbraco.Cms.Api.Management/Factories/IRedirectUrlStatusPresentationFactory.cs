using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRedirectUrlStatusPresentationFactory
{
    RedirectUrlStatusResponseModel CreateViewModel();
}
