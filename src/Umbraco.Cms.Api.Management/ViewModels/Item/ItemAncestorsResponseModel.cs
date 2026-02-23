namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public class ItemAncestorsResponseModel
{
    public Guid Id { get; set; }

    public IEnumerable<ReferenceByIdModel> Ancestors { get; set; } = Enumerable.Empty<ReferenceByIdModel>();
}
