namespace Umbraco.Cms.Web.Common.Routing;

public interface IRoutableDocumentFilter
{
    bool IsDocumentRequest(string absPath);
}
