namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContentRoute
{
    string Path { get; }

    public string? QueryString { get; set; }

    IApiContentStartItem StartItem { get; }
}
