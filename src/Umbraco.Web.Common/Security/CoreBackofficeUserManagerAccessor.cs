using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public class CoreBackofficeUserManagerAccessor : ICoreBackOfficeUserManagerAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CoreBackofficeUserManagerAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ICoreBackofficeUserManager? BackofficeUserManager
        => _httpContextAccessor.HttpContext?.RequestServices.GetService<ICoreBackofficeUserManager>();
}
