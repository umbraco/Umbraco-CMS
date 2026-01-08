using Umbraco.Extensions;

namespace Umbraco.Cms.Core;

public static class UriUtilityCore
{
    public static bool HasScheme(string uri) => uri.IndexOf("://", StringComparison.Ordinal) > 0;

    public static string StartWithScheme(string uri) => StartWithScheme(uri, null);

    public static string StartWithScheme(string uri, string? scheme) =>
        HasScheme(uri) ? uri : string.Format("{0}://{1}", scheme ?? Uri.UriSchemeHttp, uri);

    public static string EndPathWithSlash(string uri)
    {
        ReadOnlySpan<char> uriSpan = uri.AsSpan();
        var pos = IndexOfPathEnd(uriSpan);

        var path = (pos > 0 ? uriSpan[..pos] : uriSpan).ToString();
        path = path.EnsureEndsWith('/');

        if (pos > 0)
        {
            return string.Concat(path, uriSpan[pos..]);
        }

        return path;
    }

    public static string TrimPathEndSlash(string uri)
    {
        ReadOnlySpan<char> uriSpan = uri.AsSpan();
        var pos = IndexOfPathEnd(uriSpan);

        var path = (pos > 0 ? uriSpan[..pos] : uriSpan).ToString();
        path = path.TrimEnd(Constants.CharArrays.ForwardSlash);

        if (pos > 0)
        {
            return string.Concat(path, uriSpan[pos..]);
        }

        return path;
    }

    private static int IndexOfPathEnd(ReadOnlySpan<char> uri)
    {
        var pos1 = Math.Max(0, uri.IndexOf('?'));
        var pos2 = Math.Max(0, uri.IndexOf('#'));
        return pos1 == 0 && pos2 == 0 ? 0
            : pos1 == 0 ? pos2
            : pos2 == 0 ? pos1
            : Math.Min(pos1, pos2);
    }
}
