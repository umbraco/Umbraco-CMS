using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Services;

public class PreviewService : IPreviewService
{
    private readonly ICookieManager _cookieManager;

    public PreviewService(ICookieManager cookieManager) => _cookieManager = cookieManager;

    public void EnterPreview() => _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, "preview");


    public void EndPreview() => _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);
}
