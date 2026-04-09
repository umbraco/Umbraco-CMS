using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Provides IO helper implementations for macOS (OSX) operating systems.
/// </summary>
public class IOHelperOSX : IOHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IOHelperOSX"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    public IOHelperOSX(IHostingEnvironment hostingEnvironment)
        : base(hostingEnvironment)
    {
    }

    /// <inheritdoc />
    public override bool PathStartsWith(string path, string root, params char[] separators)
    {
        // either it is identical to root,
        // or it is root + separator + anything
        if (separators == null || separators.Length == 0)
        {
            separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        }

        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Length == root.Length)
        {
            return true;
        }

        if (path.Length < root.Length)
        {
            return false;
        }

        return separators.Contains(path[root.Length]);
    }
}
