using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Routing
{
    public class WebPath
    {
        /// <summary>
        /// Determines whether the provided web path is well-formed according to the specified UriKind.
        /// </summary>
        /// <param name="webPath">The web path to check. This can be null.</param>
        /// <param name="uriKind">The kind of Uri (Absolute, Relative, or RelativeOrAbsolute).</param>
        /// <returns>
        /// true if <paramref name="webPath"/> is well-formed; otherwise, false.
        /// </returns>
        public static bool IsWellFormedWebPath(string? webPath, UriKind uriKind)
        {
            if (string.IsNullOrWhiteSpace(webPath))
            {
                return false;
            }

            if (webPath.StartsWith("//"))
            {
                return uriKind is not UriKind.Relative;
            }

            return Uri.IsWellFormedUriString(webPath, uriKind);
        }
    }
}
