namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public class ItemAncestorsResponseModel<TAncestorItem>
    where TAncestorItem : ItemResponseModelBase
{
    public Guid Id { get; set; }

    public IEnumerable<TAncestorItem> Ancestors { get; set; } = Enumerable.Empty<TAncestorItem>();
}
