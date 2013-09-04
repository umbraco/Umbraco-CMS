using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Hosting;

namespace Umbraco.Web.Helpers
{
    internal class SecurityHelper
    {
        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            bool flag;
            var str = config[valueName];
            if (str == null)
                return defaultValue;
            
            if (bool.TryParse(str, out flag) == false)
            {
                throw new ProviderException("Value must be boolean.");
            }
            return flag;
        }

        /// <summary>
        /// Gets the int value.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="zeroAllowed">if set to <c>true</c> [zero allowed].</param>
        /// <param name="maxValueAllowed">The max value allowed.</param>
        /// <returns></returns>
        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            var s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (int.TryParse(s, out num) == false)
            {
                if (zeroAllowed)
                {
                    throw new ProviderException("Value must be non negative integer");
                }
                throw new ProviderException("Value must be positive integer");
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException("Value must be non negativeinteger");
            }
            if (zeroAllowed == false && (num <= 0))
            {
                throw new ProviderException("Value must be positive integer");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException("Value too big");
            }
            return num;
        }


        /// <summary>
        /// Gets the name of the default app.
        /// </summary>
        /// <returns></returns>
        internal static string GetDefaultAppName()
        {
            try
            {
                var applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                return string.IsNullOrEmpty(applicationVirtualPath) ? "/" : applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }
    }
}
