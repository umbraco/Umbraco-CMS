using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory for creating temporary file configuration presentation objects.
/// </summary>
public interface ITemporaryFileConfigurationPresentationFactory
{
    /// <summary>
    /// Creates and returns a model representing the temporary file configuration.
    /// </summary>
    /// <returns>A <see cref="TemporaryFileConfigurationResponseModel"/> instance representing the temporary file configuration.</returns>
    TemporaryFileConfigurationResponseModel Create();
}
