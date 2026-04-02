using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models of redirect URL statuses.
/// </summary>
public interface IRedirectUrlStatusPresentationFactory
{
    /// <summary>
    /// Creates and returns a view model for the redirect URL status.
    /// </summary>
    /// <returns>A <see cref="RedirectUrlStatusResponseModel"/> instance representing the redirect URL status.</returns>
    RedirectUrlStatusResponseModel CreateViewModel();
}
