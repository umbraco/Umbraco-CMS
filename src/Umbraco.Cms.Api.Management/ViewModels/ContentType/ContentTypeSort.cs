namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeSort
{
    public required ReferenceByIdModel ContentType { get; init; }

    public required int SortOrder { get; init; }
}
