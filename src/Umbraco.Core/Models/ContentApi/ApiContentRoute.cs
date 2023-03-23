namespace Umbraco.Cms.Core.Models.ContentApi;

public class ApiContentRoute : IApiContentRoute
{
    public ApiContentRoute(string path, ApiContentStartItem startItem)
    {
        Path = path;
        StartItem = startItem;
    }

    public string Path { get; }

    public IApiContentStartItem StartItem { get; }
}
