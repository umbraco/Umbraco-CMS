namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class ContentTypeComposition
{
    public required Guid Key { get; init; }

    public required ContentTypeCompositionType CompositionType { get; init; }
}
