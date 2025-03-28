using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Core.IO;

public class IOHelperWindows : IOHelper
{
    public IOHelperWindows(
        IHostingEnvironment hostingEnvironment,
        IHostEnvironment hostEnvironment)
        : base(hostingEnvironment, hostEnvironment)
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
