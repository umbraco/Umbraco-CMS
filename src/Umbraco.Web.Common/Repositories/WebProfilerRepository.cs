using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Repositories;

internal class WebProfilerRepository : IWebProfilerRepository
{
    private const string CookieName = "UMB-DEBUG";
    private const string HeaderName = "X-UMB-DEBUG";
    private const string QueryName = "umbDebug";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebProfilerRepository(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetStatus(int userId, bool status)
    {
        if (status)
        {
            _httpContextAccessor.GetRequiredHttpContext().Response.Cookies.Append(CookieName, "1", new CookieOptions { Expires = DateTime.Now.AddYears(1) });
        }
        else
        {
            _httpContextAccessor.GetRequiredHttpContext().Response.Cookies.Delete(CookieName);
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

        return request.Cookies.ContainsKey(CookieName);
    }
}
