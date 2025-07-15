namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiDocumentUrlService
{
    Guid? GetDocumentKeyByRoute(string route, string? culture, bool preview);
}
