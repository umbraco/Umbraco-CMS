namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public class ContentTypeSort
{
    public required Guid Key { get; init; }

    public required int SortOrder { get; init; }
}
