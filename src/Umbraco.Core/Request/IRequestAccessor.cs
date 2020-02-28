namespace Umbraco.Core.Request
{
    public interface IRequestAccessor
    {
        string GetRequestValue(string name);
        string GetQueryStringValue(string culture);
    }
}
