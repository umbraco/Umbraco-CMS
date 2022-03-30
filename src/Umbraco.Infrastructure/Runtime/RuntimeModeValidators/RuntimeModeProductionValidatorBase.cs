using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

public abstract class RuntimeModeProductionValidatorBase : IRuntimeModeValidator
{
    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (runtimeMode == RuntimeMode.Production)
        {
            return Validate(out validationErrorMessage);
        }

        validationErrorMessage = null;
        return true;
    }

    protected abstract bool Validate([NotNullWhen(false)] out string? validationErrorMessage);
}
