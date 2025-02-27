using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class ModelsBuilderPresentationFactory : IModelsBuilderPresentationFactory
{
    private ModelsBuilderSettings _modelsBuilderSettings;
    private readonly ModelsGenerationError _mbErrors;
    private readonly OutOfDateModelsStatus _outOfDateModels;

    public ModelsBuilderPresentationFactory(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings, ModelsGenerationError mbErrors, OutOfDateModelsStatus outOfDateModels)
    {
        _mbErrors = mbErrors;
        _outOfDateModels = outOfDateModels;
        _modelsBuilderSettings = modelsBuilderSettings.CurrentValue;

        modelsBuilderSettings.OnChange(x => _modelsBuilderSettings = x);
    }


    public ModelsBuilderResponseModel Create() =>
        new()
        {
            Mode = _modelsBuilderSettings.ModelsMode,
            CanGenerate = _modelsBuilderSettings.ModelsMode.SupportsExplicitGeneration(),
            OutOfDateModels = _outOfDateModels.IsOutOfDate,
            LastError = _mbErrors.GetLastError(),
            Version = ApiVersion.Current.Version.ToString(),
            ModelsNamespace = _modelsBuilderSettings.ModelsNamespace,
            TrackingOutOfDateModels = _modelsBuilderSettings.FlagOutOfDateModels,
        };
}
