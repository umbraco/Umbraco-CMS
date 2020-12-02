using System;
using System.IO;
using System.Linq;
using Umbraco.Core.Hosting;


namespace Umbraco.Core.IO
{
    public class IOHelperOSX : IOHelper
    {
        public IOHelperOSX(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
        }

        public override bool IsPathFullyQualified(string path) => Path.IsPathRooted(path);

        public override bool PathStartsWith(string path, string root, params char[] separators)
        {
            // either it is identical to root,
            // or it is root + separator + anything

            if (separators == null || separators.Length == 0) separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return false;
            if (path.Length == root.Length) return true;
            if (path.Length < root.Length) return false;
            return separators.Contains(path[root.Length]);
        }
    }
}
