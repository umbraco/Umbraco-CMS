namespace Umbraco.Cms.Core.Web;

public interface IRequestAccessor
{
    /// <summary>
    ///     Returns the request/form/querystring value for the given name
    /// </summary>
    string GetRequestValue(string name);

    /// <summary>
    ///     Returns the query string value for the given name
    /// </summary>
    string GetQueryStringValue(string name);

    /// <summary>
    ///     Returns the current request uri
    /// </summary>
    Uri? GetRequestUrl();
}
