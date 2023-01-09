using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Core.IO;

public class IOHelperLinux : IOHelper
{
    public IOHelperLinux(IHostingEnvironment hostingEnvironment)
        : base(hostingEnvironment)
    {
    }

    public override bool PathStartsWith(string path, string root, params char[] separators)
    {
        // either it is identical to root,
        // or it is root + separator + anything
        if (separators == null || separators.Length == 0)
        {
            separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        }

        if (!path.StartsWith(root, StringComparison.Ordinal))
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
