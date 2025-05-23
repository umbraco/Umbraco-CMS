namespace Umbraco.Cms.Core.Models.ContentEditing;

public class SortingModel
{
    public required Guid Key { get; init; }

    public required int SortOrder { get; init; }
}
