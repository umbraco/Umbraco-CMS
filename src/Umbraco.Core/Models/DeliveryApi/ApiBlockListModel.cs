namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiBlockListModel
{
    public ApiBlockListModel(IEnumerable<ApiBlockItem> items) => Items = items;

    public IEnumerable<ApiBlockItem> Items { get; }
}
