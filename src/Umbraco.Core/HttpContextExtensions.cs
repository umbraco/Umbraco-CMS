using System.Web;

namespace Umbraco.Core
{
    public static class HttpContextExtensions
    {
        public static T GetContextItem<T>(this HttpContextBase httpContext, string key)
        {
            if (httpContext == null) return default(T);
            if (httpContext.Items[key] == null) return default(T);
            var val = httpContext.Items[key].TryConvertTo<T>();
            if (val) return val.Result;
            return default(T);
        }

        public static T GetContextItem<T>(this HttpContext httpContext, string key)
        {
            return new HttpContextWrapper(httpContext).GetContextItem<T>(key);
        }

        public static string GetCurrentRequestIpAddress(this HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                return "Unknown, httpContext is null";
            }
            if (httpContext.Request == null)
            {
                return "Unknown, httpContext.Request is null";
            }
            if (httpContext.Request.ServerVariables == null)
            {
                return "Unknown, httpContext.Request.ServerVariables is null";
            }

            // From: http://stackoverflow.com/a/740431/5018

            try
            {
                var ipAddress = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                    return httpContext.Request.UserHostAddress;

                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    return addresses[0];

                return httpContext.Request.UserHostAddress;
            }
            catch (System.Exception ex)
            {
                //This try catch is to just always ensure that no matter what we're not getting any exceptions caused since
                // that would cause people to not be able to login
                return string.Format("Unknown, exception occurred trying to resolve IP {0}", ex);
            }
        }
    }
}
