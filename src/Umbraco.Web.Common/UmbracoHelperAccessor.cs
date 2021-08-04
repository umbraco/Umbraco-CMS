using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Web.Common
{
    public class UmbracoHelperAccessor : IUmbracoHelperAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UmbracoHelperAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public UmbracoHelper UmbracoHelper => _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<UmbracoHelper>();

    }
}
