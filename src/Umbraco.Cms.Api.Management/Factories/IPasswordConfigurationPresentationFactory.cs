using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models related to password configuration.
/// </summary>
public interface IPasswordConfigurationPresentationFactory
{
    /// <summary>
    /// Creates and returns a <see cref="Umbraco.Cms.Api.Management.Models.PasswordConfigurationResponseModel" /> representing the current password configuration.
    /// </summary>
    /// <returns>The password configuration response model.</returns>
    PasswordConfigurationResponseModel CreatePasswordConfigurationResponseModel();
}
