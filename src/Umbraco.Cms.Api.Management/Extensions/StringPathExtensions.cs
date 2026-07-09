using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Extensions;

internal static class StringPathExtensions
{
    /// <summary>
    /// Converts a system file path to a virtual path by replacing backslashes with forward slashes,
    /// trimming any leading '~' characters, and ensuring the path starts with a '/'.
    /// </summary>
    /// <param name="systemPath">The system file path to convert.</param>
    /// <returns>The converted virtual path.</returns>
    public static string SystemPathToVirtualPath(this string systemPath) => systemPath.Replace('\\', '/').TrimStart('~').EnsureStartsWith('/');

    /// <summary>
    /// Converts a virtual path to a system path by replacing forward slashes with the system directory separator character and ensuring the path starts with the directory separator.
    /// </summary>
    /// <param name="virtualPath">The virtual path to convert.</param>
    /// <returns>The converted system path.</returns>
    public static string VirtualPathToSystemPath(this string virtualPath) => virtualPath.Replace('/', Path.DirectorySeparatorChar).EnsureStartsWith(Path.DirectorySeparatorChar);
}
