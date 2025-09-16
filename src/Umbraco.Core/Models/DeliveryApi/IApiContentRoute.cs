namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContentRoute
{
    string Path { get; }

    public string? QueryString
    {
        get => null; set { }
    }

    IApiContentStartItem StartItem { get; }
}
