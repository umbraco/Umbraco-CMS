namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeComposition
{
    public required Guid Key { get; init; }

    public required ContentTypeCompositionType CompositionType { get; init; }
}
