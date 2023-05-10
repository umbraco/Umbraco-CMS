namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiContentRoute
{
    string Path { get; }

    IApiContentStartItem StartItem { get; }
}
