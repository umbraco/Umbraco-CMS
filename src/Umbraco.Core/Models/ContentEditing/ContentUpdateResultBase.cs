using Umbraco.Cms.Core.Models.ContentEditing.Validation;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentUpdateResultBase<TContent>
    where TContent : class, IContentBase
{
    public TContent Content { get; init; } = null!;

    public IEnumerable<PropertyValidationError> ValidationErrors { get; init; } = Enumerable.Empty<PropertyValidationError>();
}
