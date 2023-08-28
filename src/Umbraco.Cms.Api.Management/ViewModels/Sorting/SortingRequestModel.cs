namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

public class SortingRequestModel
{
    public Guid? ParentId { get; init; }

    public required IEnumerable<ItemSortingRequestModel> Sorting { get; init; }
}
