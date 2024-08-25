using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Extensions;

internal static class StringPathExtensions
{
    public static string SystemPathToVirtualPath(this string systemPath) => systemPath.Replace('\\', '/').TrimStart('~').EnsureStartsWith('/');

    public static string VirtualPathToSystemPath(this string virtualPath) => virtualPath.Replace('/', Path.DirectorySeparatorChar).EnsureStartsWith(Path.DirectorySeparatorChar);
}
