using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime;

public interface IRuntimeModeValidator
{
    bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage);
}
