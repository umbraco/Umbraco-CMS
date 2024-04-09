namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentUpdateResultBase<TContent>
    where TContent : class, IContentBase
{
    public TContent? Content { get; init; }

    public ContentValidationResult ValidationResult { get; init; } = new();
}
