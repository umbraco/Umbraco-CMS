namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class MediaTypeSort
{
    public required ReferenceByIdModel MediaType { get; init; }

    public int SortOrder { get; init; }
}
