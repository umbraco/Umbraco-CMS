namespace Umbraco.Cms.Core.Models.ContentApi;

public interface IApiContentRoute
{
    string Path { get; }

    IApiContentStartItem StartItem { get; }
}
