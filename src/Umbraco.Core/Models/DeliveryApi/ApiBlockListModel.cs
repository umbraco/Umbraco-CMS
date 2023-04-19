namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiBlockListModel
{
    public ApiBlockListModel(IEnumerable<ApiBlockItem> items) => Items = items;

    public IEnumerable<ApiBlockItem> Items { get; }
}
