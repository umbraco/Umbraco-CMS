using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

internal class RuntimeMinificationValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<RuntimeMinificationSettings> _runtimeMinificationSettings;

    public RuntimeMinificationValidator(IOptionsMonitor<RuntimeMinificationSettings> runtimeMinificationSettings)
        => _runtimeMinificationSettings = runtimeMinificationSettings;

    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Ensure runtime minification is using a fixed cache buster
        if (_runtimeMinificationSettings.CurrentValue.CacheBuster == RuntimeMinificationCacheBuster.Timestamp)
        {
            validationErrorMessage = "Runtime minification setting needs to be set to a fixed cache buster (like Version or AppDomain) in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
