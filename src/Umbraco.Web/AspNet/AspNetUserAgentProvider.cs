using Umbraco.Cms.Core.Net;

namespace Umbraco.Web.AspNet
{
    public class AspNetUserAgentProvider : IUserAgentProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetUserAgentProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserAgent()
        {
            return _httpContextAccessor.GetRequiredHttpContext().Request.UserAgent;
        }
    }
}
