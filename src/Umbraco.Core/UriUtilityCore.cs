using System;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core
{
    public static class UriUtilityCore
    {

        #region Uri string utilities

        public static bool HasScheme(string uri)
        {
            return uri.IndexOf("://") > 0;
        }

        public static string StartWithScheme(string uri)
        {
            return StartWithScheme(uri, null);
        }

        public static string StartWithScheme(string uri, string? scheme)
        {
            return HasScheme(uri) ? uri : String.Format("{0}://{1}", scheme ?? Uri.UriSchemeHttp, uri);
        }

        public static string EndPathWithSlash(string uri)
        {
            var pos1 = Math.Max(0, uri.IndexOf('?'));
            var pos2 = Math.Max(0, uri.IndexOf('#'));
            var pos = Math.Min(pos1, pos2);

            var path = pos > 0 ? uri.Substring(0, pos) : uri;
            path = path.EnsureEndsWith('/');

            if (pos > 0)
                path += uri.Substring(pos);

            return path;
        }

        public static string TrimPathEndSlash(string uri)
        {
            var pos1 = Math.Max(0, uri.IndexOf('?'));
            var pos2 = Math.Max(0, uri.IndexOf('#'));
            var pos = Math.Min(pos1, pos2);

            var path = pos > 0 ? uri.Substring(0, pos) : uri;
            path = path.TrimEnd(Constants.CharArrays.ForwardSlash);

            if (pos > 0)
                path += uri.Substring(pos);

            return path;
        }

        #endregion

    }
}
