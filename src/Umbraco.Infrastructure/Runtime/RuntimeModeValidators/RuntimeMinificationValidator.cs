using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether the runtime minification cache buster is not set to <see cref="RuntimeMinificationCacheBuster.Timestamp" /> when in production runtime mode.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators.RuntimeModeProductionValidatorBase" />
public class RuntimeMinificationValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<RuntimeMinificationSettings> _runtimeMinificationSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeMinificationValidator" /> class.
    /// </summary>
    /// <param name="runtimeMinificationSettings">The runtime minification settings.</param>
    public RuntimeMinificationValidator(IOptionsMonitor<RuntimeMinificationSettings> runtimeMinificationSettings)
        => _runtimeMinificationSettings = runtimeMinificationSettings;

    /// <inheritdoc />
    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (_runtimeMinificationSettings.CurrentValue.CacheBuster == RuntimeMinificationCacheBuster.Timestamp)
        {
            validationErrorMessage = "Runtime minification setting needs to be set to a fixed cache buster (like Version or AppDomain) in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
