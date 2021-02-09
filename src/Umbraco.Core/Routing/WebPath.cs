using System;
using System.Linq;

namespace Umbraco.Cms.Core.Routing
{
    public class WebPath
    {
        public static string Combine(params string[] paths)
        {
            const string separator = "/";

            if (paths == null) throw new ArgumentNullException(nameof(paths));
            if (!paths.Any()) return string.Empty;



            var result = paths[0].TrimEnd(separator);

            if(!(result.StartsWith(separator) || result.StartsWith("~" + separator)))
            {
                result = separator + result;
            }

            for (var index = 1; index < paths.Length; index++)
            {

                result +=separator + paths[index].Trim(separator);
            }

            return result;
        }
    }
}
