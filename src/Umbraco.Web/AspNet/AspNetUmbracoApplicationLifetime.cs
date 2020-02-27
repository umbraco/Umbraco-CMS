using Umbraco.Net;

namespace Umbraco.Web.AspNet
{
    public class AspNetUmbracoApplicationLifetime : IUmbracoApplicationLifetime
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetUmbracoApplicationLifetime(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Restart()
        {
            UmbracoApplication.Restart(_httpContextAccessor.HttpContext);
        }
    }
}
