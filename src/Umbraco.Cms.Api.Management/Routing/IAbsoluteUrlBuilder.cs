namespace Umbraco.Cms.Api.Management.Routing;

public interface IAbsoluteUrlBuilder
{
    Uri ToAbsoluteUrl(string url);
}
