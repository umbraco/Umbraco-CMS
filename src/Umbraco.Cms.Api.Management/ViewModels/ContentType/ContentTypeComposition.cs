namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeComposition
{
    public required ReferenceByIdModel ContentType { get; init; }

    public required ContentTypeCompositionType CompositionType { get; init; }
}
