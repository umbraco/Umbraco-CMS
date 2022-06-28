using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates configuration based on the production runtime mode.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.IRuntimeModeValidator" />
public abstract class RuntimeModeProductionValidatorBase : IRuntimeModeValidator
{
    /// <inheritdoc />
    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (runtimeMode == RuntimeMode.Production)
        {
            return Validate(out validationErrorMessage);
        }

        validationErrorMessage = null;
        return true;
    }

    /// <summary>
    /// Validates configuration based on the production runtime mode.
    /// </summary>
    /// <param name="validationErrorMessage">The validation error message.</param>
    /// <returns>
    ///   <c>true</c> when the validation passes; otherwise, <c>false</c>.
    /// </returns>
    protected abstract bool Validate([NotNullWhen(false)] out string? validationErrorMessage);
}
