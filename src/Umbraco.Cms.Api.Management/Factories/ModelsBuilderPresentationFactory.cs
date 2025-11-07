using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Api.Management.Factories;

public class ModelsBuilderPresentationFactory : IModelsBuilderPresentationFactory
{
    private readonly ModelsGenerationError _mbErrors;
    private readonly OutOfDateModelsStatus _outOfDateModels;
    private readonly ModelsBuilderSettings _modelsBuilderSettings;

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
