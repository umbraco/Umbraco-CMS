using System;
using System.Text;

namespace Umbraco.Core
{
    internal static class UriExtensions
    {
        public static Uri Rewrite(this Uri uri, string path, string query)
        {
            var pathAndQuery = new StringBuilder();

            if (!path.StartsWith("/"))
                pathAndQuery.Append("/");
            pathAndQuery.Append(path);
            if (!query.StartsWith("?"))
                pathAndQuery.Append("?");
            pathAndQuery.Append(query);

            return new Uri(uri, pathAndQuery.ToString());
        }
    }
}