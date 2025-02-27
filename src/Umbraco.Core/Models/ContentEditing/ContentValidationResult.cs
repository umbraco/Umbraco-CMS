using Umbraco.Cms.Core.Models.ContentEditing.Validation;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public class ContentValidationResult
{
    public IEnumerable<PropertyValidationError> ValidationErrors { get; init; } = Enumerable.Empty<PropertyValidationError>();
}
