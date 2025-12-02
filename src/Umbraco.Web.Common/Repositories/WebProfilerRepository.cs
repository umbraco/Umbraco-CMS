using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Repositories;

internal sealed class WebProfilerRepository : IWebProfilerRepository
{
    private const string CookieName = "UMB-DEBUG";
    private const string HeaderName = "X-UMB-DEBUG";
    private const string QueryName = "umbDebug";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICookieManager _cookieManager;

    public WebProfilerRepository(IHttpContextAccessor httpContextAccessor, ICookieManager cookieManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieManager = cookieManager;
    }

    public void SetStatus(int userId, bool status)
    {
        if (status)
        {
            // This cookie does not need to be secure as it is only used for local debugging purposes.
            // It is also marked as SameSite=Strict to avoid being sent in cross-site requests, which means
            // it will not be sent when the BackOffice is hosted on a different domain/port than the frontend.
            // The intention is to force debug profiling on without any code snippets, but as it can only be activated from
            // the local BackOffice, it will unfortunately only work in same-site scenarios.
            // If the user wants to debug profile cross-site, they can use the query string or header options.
            _cookieManager.SetCookieValue(
                CookieName,
                "1",
                httpOnly: true,
                secure: false,
                sameSiteMode: "Strict");
        }
        else
        {
            _cookieManager.ExpireCookie(CookieName);
        }
    }

    public bool GetStatus(int userId)
    {

        var request = _httpContextAccessor.GetRequiredHttpContext().Request;
        if (bool.TryParse(request.Query[QueryName], out var umbDebug))
        {
            return umbDebug;
        }

        if (bool.TryParse(request.Headers[HeaderName], out var xUmbDebug))
        {
            return xUmbDebug;
        }

        return _cookieManager.HasCookie(CookieName);
    }
}
