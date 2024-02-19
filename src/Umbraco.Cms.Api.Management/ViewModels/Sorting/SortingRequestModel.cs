namespace Umbraco.Cms.Api.Management.ViewModels.Sorting;

public class SortingRequestModel
{
    public ReferenceByIdModel? Parent { get; init; }

    public required IEnumerable<ItemSortingRequestModel> Sorting { get; init; }
}
