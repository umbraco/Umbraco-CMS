using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <inheritdoc />
internal class RuntimeModeValidationService : IRuntimeModeValidationService
{
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeModeValidationService" /> class.
    /// </summary>
    /// <param name="runtimeSettings">The runtime settings.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public RuntimeModeValidationService(IOptionsMonitor<RuntimeSettings> runtimeSettings, IServiceProvider serviceProvider)
    {
        _runtimeSettings = runtimeSettings;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        var runtimeMode = _runtimeSettings.CurrentValue.Mode;
        var validationMessages = new List<string>();

        // Runtime mode validators are registered transient, but this service is registered as singleton
        using (var scope = _serviceProvider.CreateScope())
        {
            var runtimeModeValidators = scope.ServiceProvider.GetService<RuntimeModeValidatorCollection>();
            if (runtimeModeValidators is not null)
            {
                foreach (var runtimeModeValidator in runtimeModeValidators)
                {
                    if (runtimeModeValidator.Validate(runtimeMode, out var validationMessage) == false)
                    {
                        validationMessages.Add(validationMessage);
                    }
                }
            }
        }

        if (validationMessages.Count > 0)
        {
            validationErrorMessage = $"Runtime mode validation failed for {runtimeMode}:\n" + string.Join("\n", validationMessages);
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
