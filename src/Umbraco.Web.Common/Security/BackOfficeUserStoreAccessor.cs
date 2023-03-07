using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.Security;

public class BackOfficeUserStoreAccessor : IBackOfficeUserStoreAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BackOfficeUserStoreAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IBackofficeUserStore? BackOfficeUserStore
        => _httpContextAccessor.HttpContext?.RequestServices.GetService<IBackofficeUserStore>();
}
