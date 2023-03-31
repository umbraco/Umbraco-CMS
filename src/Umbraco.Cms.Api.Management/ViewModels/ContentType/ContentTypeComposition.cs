namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeComposition
{
    public required Guid Id { get; init; }

    public required ContentTypeCompositionType CompositionType { get; init; }
}
