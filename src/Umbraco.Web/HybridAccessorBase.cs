using System;

namespace Umbraco.Web
{
    // fixme - must ensure that the ThreadStatic value is properly cleared!
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
                if (httpContext == null) return _value;
                return (T) httpContext.Items[HttpContextItemKey];
            }

            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _value = value;
                    return;
                }
                httpContext.Items[HttpContextItemKey] = value;
            }
        }
    }
}