using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether the JIT/runtime optimizer of the entry assembly is enabled in production runtime mode.
/// </summary>
/// <remarks>
/// This can be ensured by building the application using the Release configuration.
/// </remarks>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators.RuntimeModeProductionValidatorBase" />
public class JITOptimizerValidator : RuntimeModeProductionValidatorBase
{
    /// <inheritdoc />
    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        DebuggableAttribute? debuggableAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<DebuggableAttribute>();
        if (debuggableAttribute != null && debuggableAttribute.IsJITOptimizerDisabled)
        {
            validationErrorMessage = "The JIT/runtime optimizer of the entry assembly needs to be enabled in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
