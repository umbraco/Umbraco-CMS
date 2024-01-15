using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class MediaTypeComposition
{
    public required ReferenceByIdModel MediaType { get; init; }

    public required CompositionType CompositionType { get; init; }
}
