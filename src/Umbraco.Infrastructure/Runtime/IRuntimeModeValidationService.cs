using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Infrastructure.Runtime;

public interface IRuntimeModeValidationService
{
    bool Validate([NotNullWhen(false)] out string? validationErrorMessage);
}
