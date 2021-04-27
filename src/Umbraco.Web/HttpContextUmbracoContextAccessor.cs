using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Web
{
    internal class HttpContextUmbracoContextAccessor : IUmbracoContextAccessor
    {
        private readonly IRequestCache _requestCache;
        private const string HttpContextItemKey = "Umbraco.Web.UmbracoContext";

        public HttpContextUmbracoContextAccessor(IRequestCache requestCache)
        {
            _requestCache = requestCache;
        }

        public IUmbracoContext UmbracoContext
        {
            get
            {
                if (!_requestCache.IsAvailable) throw new InvalidOperationException("No request cache available");
                return (IUmbracoContext) _requestCache.Get(HttpContextItemKey);
            }
            set
            {
                if (!_requestCache.IsAvailable)  throw new InvalidOperationException("No request cache available");
                _requestCache.Set(HttpContextItemKey, value);
            }
        }
    }
}
