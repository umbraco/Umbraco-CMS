using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Repositories;

internal class WebProfilerRepository : IWebProfilerRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CookieName = "UMB-DEBUG";

    public WebProfilerRepository(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetStatus(int userId, bool status)
    {
        if (status)
        {
            _httpContextAccessor.GetRequiredHttpContext().Response.Cookies.Append(CookieName, string.Empty, new CookieOptions { Expires = DateTime.Now.AddYears(1) });
        }
        else
        {
            _httpContextAccessor.GetRequiredHttpContext().Response.Cookies.Delete(CookieName);
        }
    }

    public bool GetStatus(int userId) => _httpContextAccessor.GetRequiredHttpContext().Request.Cookies.ContainsKey(CookieName);
}
