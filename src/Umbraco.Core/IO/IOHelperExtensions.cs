using System;
using System.IO;

namespace Umbraco.Core.IO
{
    public static class IOHelperExtensions
    {
        private static string _mvcArea;

        /// <summary>
        /// Tries to create a directory.
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
                    Directory.CreateDirectory(dirPath);

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

        public static string CreateRandomFileName(this IIOHelper ioHelper)
        {
            return "umbraco-test." + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// This will return the MVC area that we will route all custom routes through like surface controllers, etc...
        /// We will use the 'Path' (default ~/umbraco) to create it but since it cannot contain '/' and people may specify a path of ~/asdf/asdf/admin
        /// we will convert the '/' to '-' and use that as the path. its a bit lame but will work.
        ///
        /// We also make sure that the virtual directory (SystemDirectories.Root) is stripped off first, otherwise we'd end up with something
        /// like "MyVirtualDirectory-Umbraco" instead of just "Umbraco".
        /// </remarks>
        public static string GetUmbracoMvcArea(this IIOHelper ioHelper)
        {
            if (_mvcArea != null) return _mvcArea;

            _mvcArea = GetUmbracoMvcAreaNoCache(ioHelper);

            return _mvcArea;
        }

        internal static string GetUmbracoMvcAreaNoCache(this IIOHelper ioHelper)
        {
            if (ioHelper.BackOfficePath.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
            }

            var path = ioHelper.BackOfficePath;
            if (path.StartsWith(ioHelper.Root)) // beware of TrimStart, see U4-2518
                path = path.Substring(ioHelper.Root.Length);
            return path.TrimStart('~').TrimStart('/').Replace('/', '-').Trim().ToLower();
        }
    }
}
