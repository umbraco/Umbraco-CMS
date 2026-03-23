using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory for creating presentation models used by the Models Builder.
/// </summary>
public interface IModelsBuilderPresentationFactory
{
    /// <summary>
    /// Creates and returns a new <see cref="ModelsBuilderResponseModel"/> instance.
    /// </summary>
    /// <returns>The created <see cref="ModelsBuilderResponseModel"/>.</returns>
    ModelsBuilderResponseModel Create();
}
