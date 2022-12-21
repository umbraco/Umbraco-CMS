namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiBlockListModel
{
    public ApiBlockListModel(IEnumerable<ApiBlockItem> items) => Items = items;

    public IEnumerable<ApiBlockItem> Items { get; }
}
