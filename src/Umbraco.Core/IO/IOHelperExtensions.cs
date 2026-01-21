using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IIOHelper"/> for common IO operations.
/// </summary>
public static class IOHelperExtensions
{
    /// <summary>
    ///     Will resolve a virtual path URL to an absolute path, else if it is not a virtual path (i.e. starts with ~/) then
    ///     it will just return the path as-is (relative).
    /// </summary>
    /// <param name="ioHelper"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull("path")]
    public static string? ResolveRelativeOrVirtualUrl(this IIOHelper ioHelper, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return path.StartsWith("~/") ? ioHelper.ResolveUrl(path) : path;
    }

    /// <summary>
    ///     Tries to create a directory.
    /// </summary>
    /// <param name="ioHelper">The IOHelper.</param>
    /// <param name="dir">the directory path.</param>
    /// <returns>true if the directory was created, false otherwise.</returns>
    public static bool TryCreateDirectory(this IIOHelper ioHelper, string dir)
    {
        try
        {
            var dirPath = ioHelper.MapPath(dir);

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = dirPath + "/" + CreateRandomFileName(ioHelper) + ".tmp";
            File.WriteAllText(filePath, "This is an Umbraco internal test file. It is safe to delete it.");
            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a random temporary file name for testing purposes.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    /// <returns>A random file name in the format "umbraco-test.{8-char-guid}".</returns>
    public static string CreateRandomFileName(this IIOHelper ioHelper) =>
        "umbraco-test." + Guid.NewGuid().ToString("N").Substring(0, 8);
}
