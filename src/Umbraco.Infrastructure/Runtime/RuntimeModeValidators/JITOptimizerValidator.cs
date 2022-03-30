using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

internal class JITOptimizerValidator : RuntimeModeProductionValidatorBase
{
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
