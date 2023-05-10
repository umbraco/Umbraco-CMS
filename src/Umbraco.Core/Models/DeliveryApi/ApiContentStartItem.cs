namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiContentStartItem : IApiContentStartItem
{
    public ApiContentStartItem(Guid id, string path)
    {
        Id = id;
        Path = path;
    }

    public Guid Id { get; }

    public string Path { get; }
}
