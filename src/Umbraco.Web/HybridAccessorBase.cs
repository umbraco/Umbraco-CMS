using System;

namespace Umbraco.Web
{
    internal abstract class HybridAccessorBase<T>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected abstract string HttpContextItemKey { get; }

        [ThreadStatic]
        private static T _value;

        protected HybridAccessorBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected T Value
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return _value; //throw new Exception("oops:httpContext");
                return (T) httpContext.Items[HttpContextItemKey];
            }

            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) //throw new Exception("oops:httpContext");
                {
                    _value = value;
                    return;
                }
                httpContext.Items[HttpContextItemKey] = value;
            }
        }
    }
}