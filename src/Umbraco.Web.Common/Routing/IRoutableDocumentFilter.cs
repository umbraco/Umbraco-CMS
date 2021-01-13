namespace Umbraco.Web.Common.Routing
{
    public interface IRoutableDocumentFilter
    {
        bool IsDocumentRequest(string absPath);
    }
}