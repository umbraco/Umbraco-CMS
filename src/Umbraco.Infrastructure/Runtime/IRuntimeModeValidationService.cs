using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <summary>
/// Provides a service to validate configuration based on the runtime mode.
/// </summary>
public interface IRuntimeModeValidationService
{
    /// <summary>
    /// Validates configuration based on the runtime mode.
    /// </summary>
    /// <param name="validationErrorMessage">The validation error message.</param>
    /// <returns>
    ///   <c>true</c> when the validation passes; otherwise, <c>false</c>.
    /// </returns>
    bool Validate([NotNullWhen(false)] out string? validationErrorMessage);
}
