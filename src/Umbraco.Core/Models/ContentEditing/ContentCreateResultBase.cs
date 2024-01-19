using Umbraco.Cms.Core.Models.ContentEditing.Validation;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentCreateResultBase<TContent>
    where TContent : class, IContentBase
{
    public TContent? Content { get; init; }

    public IEnumerable<PropertyValidationError> ValidationErrors { get; init; } = Enumerable.Empty<PropertyValidationError>();
}
