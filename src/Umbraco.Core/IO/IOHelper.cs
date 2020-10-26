using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Hosting;
using Umbraco.Core.Strings;

namespace Umbraco.Core.IO
{
    public class IOHelper : IIOHelper
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public IOHelper(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        // static compiled regex for faster performance
        //private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        //helper to try and match the old path to a new virtual one
        public string FindFile(string virtualPath)
        {
            string retval = virtualPath;

            if (virtualPath.StartsWith("~"))
                retval = virtualPath.Replace("~", _hostingEnvironment.ApplicationVirtualPath);

            if (virtualPath.StartsWith("/") && virtualPath.StartsWith(_hostingEnvironment.ApplicationVirtualPath) == false)
                retval = _hostingEnvironment.ApplicationVirtualPath + "/" + virtualPath.TrimStart('/');

            return retval;
        }

        // TODO: This is the same as IHostingEnvironment.ToAbsolute
        public string ResolveUrl(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath)) return virtualPath;
            return _hostingEnvironment.ToAbsolute(virtualPath);

        }

        public Attempt<string> TryResolveUrl(string virtualPath)
        {
            try
            {
                if (virtualPath.StartsWith("~"))
                    return Attempt.Succeed(virtualPath.Replace("~", _hostingEnvironment.ApplicationVirtualPath).Replace("//", "/"));
                if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
                    return Attempt.Succeed(virtualPath);

                return Attempt.Succeed(_hostingEnvironment.ToAbsolute(virtualPath));
            }
            catch (Exception ex)
            {
                return Attempt.Fail(virtualPath, ex);
            }
        }

        public string MapPath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Check if the path is already mapped
            if ((path.Length >= 2 && path[1] == Path.VolumeSeparatorChar)
                || path.StartsWith(@"\\")) //UNC Paths start with "\\". If the site is running off a network drive mapped paths will look like "\\Whatever\Boo\Bar"
            {
                return path;
            }
            // Check that we even have an HttpContext! otherwise things will fail anyways
            // http://umbraco.codeplex.com/workitem/30946


            if (_hostingEnvironment.IsHosted)
            {
                var result = (!string.IsNullOrEmpty(path) && (path.StartsWith("~") || path.StartsWith(_hostingEnvironment.ApplicationVirtualPath)))
                    ? _hostingEnvironment.MapPathWebRoot(path)
                    : _hostingEnvironment.MapPathWebRoot("~/" + path.TrimStart('/'));

                if (result != null) return result;
            }

            var dirSepChar = Path.DirectorySeparatorChar;
            var root = Assembly.GetExecutingAssembly().GetRootDirectorySafe();
            var newPath = path.TrimStart('~', '/').Replace('/', dirSepChar);
            var retval = root + dirSepChar.ToString(CultureInfo.InvariantCulture) + newPath;

            return retval;
        }


        /// <summary>
        /// Verifies that the current filepath matches a directory where the user is allowed to edit a file.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validDir">The valid directory.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        public bool VerifyEditPath(string filePath, string validDir)
        {
            return VerifyEditPath(filePath, new[] { validDir });
        }

        /// <summary>
        /// Verifies that the current filepath matches one of several directories where the user is allowed to edit a file.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validDirs">The valid directories.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        public bool VerifyEditPath(string filePath, IEnumerable<string> validDirs)
        {
            // this is called from ScriptRepository, PartialViewRepository, etc.
            // filePath is the fullPath (rooted, filesystem path, can be trusted)
            // validDirs are virtual paths (eg ~/Views)
            //
            // except that for templates, filePath actually is a virtual path

            // TODO: what's below is dirty, there are too many ways to get the root dir, etc.
            // not going to fix everything today

            var mappedRoot = MapPath(_hostingEnvironment.ApplicationVirtualPath);
            if (filePath.StartsWith(mappedRoot) == false)
                filePath = _hostingEnvironment.MapPathContentRoot(filePath);

            // yes we can (see above)
            //// don't trust what we get, it may contain relative segments
            //filePath = Path.GetFullPath(filePath);

            foreach (var dir in validDirs)
            {
                var validDir = dir;
                if (validDir.StartsWith(mappedRoot) == false)
                    validDir = _hostingEnvironment.MapPathContentRoot(validDir);

                if (PathStartsWith(filePath, validDir, Path.DirectorySeparatorChar))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Verifies that the current filepath has one of several authorized extensions.
        /// </summary>
        /// <param name="filePath">The filepath to validate.</param>
        /// <param name="validFileExtensions">The valid extensions.</param>
        /// <returns>A value indicating whether the filepath is valid.</returns>
        public bool VerifyFileExtension(string filePath, IEnumerable<string> validFileExtensions)
        {
            var ext = Path.GetExtension(filePath);
            return ext != null && validFileExtensions.Contains(ext.TrimStart('.'));
        }

        public bool PathStartsWith(string path, string root, char separator)
        {
            // either it is identical to root,
            // or it is root + separator + anything

            if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase) == false) return false;
            if (path.Length == root.Length) return true;
            if (path.Length < root.Length) return false;
            return path[root.Length] == separator;
        }

        public void EnsurePathExists(string path)
        {
            var absolutePath = MapPath(path);
            if (Directory.Exists(absolutePath) == false)
                Directory.CreateDirectory(absolutePath);
        }

        /// <summary>
        /// Get properly formatted relative path from an existing absolute or relative path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetRelativePath(string path)
        {
            if (path.IsFullPath())
            {
                var rootDirectory = MapPath("~");
                var relativePath = path.ToLowerInvariant().Replace(rootDirectory.ToLowerInvariant(), string.Empty);
                path = relativePath;
            }

            return PathUtility.EnsurePathIsApplicationRootPrefixed(path);
        }

    }
}
