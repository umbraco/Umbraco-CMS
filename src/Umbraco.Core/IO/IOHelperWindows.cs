using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.IO;

public class IOHelperWindows : IOHelper
{
    public IOHelperWindows(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
    {
    }

    public override bool IsPathFullyQualified(string path)
    {
        // TODO: This implementation is taken from the .NET Standard 2.1 implementation.  We should switch to using Path.IsPathFullyQualified once we are on .NET Standard 2.1

        if (path.Length < 2)
        {
            // It isn't fixed, it must be relative.  There is no way to specify a fixed
            // path with one character (or less).
            return false;
        }

        if (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar)
        {
            // There is no valid way to specify a relative path with two initial slashes or
            // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
            return path[1] == '?' || path[1] == Path.DirectorySeparatorChar ||
                   path[1] == Path.AltDirectorySeparatorChar;
        }

        // The only way to specify a fixed path that doesn't begin with two slashes
        // is the drive, colon, slash format- i.e. C:\
        return path.Length >= 3
               && path[1] == Path.VolumeSeparatorChar
               && (path[2] == Path.DirectorySeparatorChar || path[2] == Path.AltDirectorySeparatorChar)
               // To match old behavior we'll check the drive character for validity as the path is technically
               // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
               && ((path[0] >= 'A' && path[0] <= 'Z') || (path[0] >= 'a' && path[0] <= 'z'));
    }

    public override bool PathStartsWith(string path, string root, params char[] separators)
    {
        // either it is identical to root,
        // or it is root + separator + anything

        if (separators == null || separators.Length == 0)
        {
            separators = new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
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
