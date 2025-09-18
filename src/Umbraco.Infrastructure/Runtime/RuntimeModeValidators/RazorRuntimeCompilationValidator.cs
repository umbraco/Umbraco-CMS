using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

public class RazorRuntimeCompilationValidator : IRuntimeModeValidator
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public RazorRuntimeCompilationValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
    }

    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (_modelsBuilderSettings.CurrentValue.ModelsMode == "InMemoryAuto" && _publishedModelFactory.IsLiveFactoryEnabled() is false)
        {
            validationErrorMessage = "InMemoryAuto requires the Umbraco.Cms.DevelopmentMode.Backoffice package to be installed.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
