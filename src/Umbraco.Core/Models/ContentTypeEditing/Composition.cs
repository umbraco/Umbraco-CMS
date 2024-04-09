namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class Composition
{
    public required Guid Key { get; init; }

    public required CompositionType CompositionType { get; init; }
}
