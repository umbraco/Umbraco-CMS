using Umbraco.Core.Request;

namespace Umbraco.Web.AspNet
{
    public class AspNetRequestAccessor : IRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetRequestAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetRequestValue(string name)
        {
            return _httpContextAccessor.GetRequiredHttpContext().Request[name];
        }

        public string GetQueryStringValue(string name)
        {
            return _httpContextAccessor.GetRequiredHttpContext().Request.QueryString[name];
        }
    }
}
