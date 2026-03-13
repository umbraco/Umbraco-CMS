using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for the Models Builder feature in Umbraco.
/// </summary>
public class ModelsBuilderPresentationFactory : IModelsBuilderPresentationFactory
{
    private readonly ModelsGenerationError _mbErrors;
    private readonly OutOfDateModelsStatus _outOfDateModels;
    private readonly ModelsBuilderSettings _modelsBuilderSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.ModelsBuilderPresentationFactory"/> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">An options monitor that provides access to the current <see cref="ModelsBuilderSettings"/> configuration.</param>
    /// <param name="mbErrors">An instance of <see cref="ModelsGenerationError"/> used to track errors during model generation.</param>
    /// <param name="outOfDateModels">An instance of <see cref="OutOfDateModelsStatus"/> indicating whether generated models are out of date.</param>
    public ModelsBuilderPresentationFactory(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings, ModelsGenerationError mbErrors, OutOfDateModelsStatus outOfDateModels)
    {
        _mbErrors = mbErrors;
        _outOfDateModels = outOfDateModels;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;
    }

    public ModelsBuilderResponseModel Create() =>
        new()
        {
            Mode = _modelsBuilderSettings.ModelsMode,
            CanGenerate = _modelsBuilderSettings.ModelsMode is Constants.ModelsBuilder.ModelsModes.SourceCodeManual or Constants.ModelsBuilder.ModelsModes.SourceCodeAuto,
            OutOfDateModels = _outOfDateModels.IsOutOfDate,
            LastError = _mbErrors.GetLastError(),
            Version = ApiVersion.Current.Version.ToString(),
            ModelsNamespace = _modelsBuilderSettings.ModelsNamespace,
            TrackingOutOfDateModels = _modelsBuilderSettings.FlagOutOfDateModels,
        };
}
