using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Services;

public class PreviewService : IPreviewService
{
    private readonly ICookieManager _cookieManager;

    public PreviewService(ICookieManager cookieManager) => _cookieManager = cookieManager;

    public void EnterPreview() => _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, "preview");


    public void ExitPreview()
    {
        _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);

        // Expire Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
        _cookieManager.ExpireCookie(Constants.Web.AcceptPreviewCookieName);
    }
}
