using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Extensions;

// TODO: this figure out a better (filesystem based) approach for this - or as a very bare minimum make this injectable; this should NOT BE AN EXTENSION!
internal static class StringPathExtensions
{
    public static string SystemPathToVirtualPath(this string systemPath) => systemPath.Replace('\\', '/').TrimStart('~').EnsureStartsWith('/');

    public static string VirtualPathToSystemPath(this string virtualPath) => virtualPath.Replace('/', Path.DirectorySeparatorChar).EnsureStartsWith(Path.DirectorySeparatorChar);

    public static string? ParentPath(this string path) => Path.GetDirectoryName(path);
}
