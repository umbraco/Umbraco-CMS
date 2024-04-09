namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentCreateResultBase<TContent>
    where TContent : class, IContentBase
{
    public TContent? Content { get; init; }

    public ContentValidationResult ValidationResult { get; init; } = new();
}
