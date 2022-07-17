using Microsoft.Extensions.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
///     Contains extension methods for the <see cref="IHostEnvironment" /> interface.
/// </summary>
public static class HostEnvironmentExtensions
{
    private static string? _temporaryApplicationId;

    /// <summary>
    ///     Maps a virtual path to a physical path to the application's content root.
    /// </summary>
    /// <remarks>
    ///     Generally the content root is the parent directory of the web root directory.
    /// </remarks>
    public static string MapPathContentRoot(this IHostEnvironment hostEnvironment, string path)
    {
        var root = hostEnvironment.ContentRootPath;

        var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // TODO: This is a temporary error because we switched from IOHelper.MapPath to HostingEnvironment.MapPathXXX
        // IOHelper would check if the path passed in started with the root, and not prepend the root again if it did,
        // however if you are requesting a path be mapped, it should always assume the path is relative to the root, not
        // absolute in the file system.  This error will help us find and fix improper uses, and should be removed once
        // all those uses have been found and fixed
        if (newPath.StartsWith(root))
        {
            throw new ArgumentException(
                "The path appears to already be fully qualified.  Please remove the call to MapPathContentRoot");
        }

        return Path.Combine(root, newPath.TrimStart(Constants.CharArrays.TildeForwardSlashBackSlash));
    }

    /// <summary>
    ///     Gets a temporary application id for use before the ioc container is built.
    /// </summary>
    public static string GetTemporaryApplicationId(this IHostEnvironment hostEnvironment)
    {
        if (_temporaryApplicationId != null)
        {
            return _temporaryApplicationId;
        }

        return _temporaryApplicationId = hostEnvironment.ContentRootPath.GenerateHash();
    }
}
