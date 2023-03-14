using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <summary>
/// Validates configuration based on the runtime mode.
/// </summary>
public interface IRuntimeModeValidator
{
    /// <summary>
    /// Validates configuration based on the specified <paramref name="runtimeMode" />.
    /// </summary>
    /// <param name="runtimeMode">The runtime mode.</param>
    /// <param name="validationErrorMessage">The validation error message.</param>
    /// <returns>
    ///   <c>true</c> when the validation passes; otherwise, <c>false</c>.
    /// </returns>
    bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage);
}
