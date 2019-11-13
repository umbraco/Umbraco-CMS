using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Umbraco.Core.IO
{
    public class IOHelper : IIOHelper
    {
        internal static IIOHelper Default { get; } = new IOHelper();

        /// <summary>
        /// Gets or sets a value forcing Umbraco to consider it is non-hosted.
        /// </summary>
        /// <remarks>This should always be false, unless unit testing.</remarks>
	    public bool ForceNotHosted { get; set; }

        private static string _rootDir = "";

        // static compiled regex for faster performance
        //private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Gets a value indicating whether Umbraco is hosted.
        /// </summary>
	    public bool IsHosted => !ForceNotHosted && (HttpContext.Current != null || HostingEnvironment.IsHosted);

        public char DirSepChar => Path.DirectorySeparatorChar;

        //helper to try and match the old path to a new virtual one
        public string FindFile(string virtualPath)
        {
            string retval = virtualPath;

            if (virtualPath.StartsWith("~"))
                retval = virtualPath.Replace("~", Root);

            if (virtualPath.StartsWith("/") && virtualPath.StartsWith(Root) == false)
                retval = Root + "/" + virtualPath.TrimStart('/');

            return retval;
        }

        public string ResolveVirtualUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            return path.StartsWith("~/") ? ResolveUrl(path) : path;
        }

        //Replaces tildes with the root dir
        public string ResolveUrl(string virtualPath)
        {
            if (virtualPath.StartsWith("~"))
                return virtualPath.Replace("~", Root).Replace("//", "/");
            else if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
                return virtualPath;
            else
                return VirtualPathUtility.ToAbsolute(virtualPath, Root);
        }

        public Attempt<string> TryResolveUrl(string virtualPath)
        {
            try
            {
                if (virtualPath.StartsWith("~"))
                    return Attempt.Succeed(virtualPath.Replace("~", Root).Replace("//", "/"));
                if (Uri.IsWellFormedUriString(virtualPath, UriKind.Absolute))
                    return Attempt.Succeed(virtualPath);
                return Attempt.Succeed(VirtualPathUtility.ToAbsolute(virtualPath, Root));
            }
            catch (Exception ex)
            {
                return Attempt.Fail(virtualPath, ex);
            }
        }

        public string MapPath(string path, bool useHttpContext)
        {
            if (path == null) throw new ArgumentNullException("path");
            useHttpContext = useHttpContext && IsHosted;

            // Check if the path is already mapped
            if ((path.Length >= 2 && path[1] == Path.VolumeSeparatorChar)
                || path.StartsWith(@"\\")) //UNC Paths start with "\\". If the site is running off a network drive mapped paths will look like "\\Whatever\Boo\Bar"
            {
                return path;
            }
            // Check that we even have an HttpContext! otherwise things will fail anyways
            // http://umbraco.codeplex.com/workitem/30946

            if (useHttpContext && HttpContext.Current != null)
            {
                //string retval;
                if (String.IsNullOrEmpty(path) == false && (path.StartsWith("~") || path.StartsWith(Root)))
                    return HostingEnvironment.MapPath(path);
                else
                    return HostingEnvironment.MapPath("~/" + path.TrimStart('/'));
            }

            var root = GetRootDirectorySafe();
            var newPath = path.TrimStart('~', '/').Replace('/', DirSepChar);
            var retval = root + DirSepChar.ToString(CultureInfo.InvariantCulture) + newPath;

            return retval;
        }

        public string MapPath(string path)
        {
            return MapPath(path, true);
        }

        //use a tilde character instead of the complete path
        public string ReturnPath(string settingsKey, string standardPath, bool useTilde)
        {
            var retval = ConfigurationManager.AppSettings[settingsKey];

            if (string.IsNullOrEmpty(retval))
                retval = standardPath;

            return retval.TrimEnd('/');
        }

        public string ReturnPath(string settingsKey, string standardPath)
        {
            return ReturnPath(settingsKey, standardPath, false);

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

            var mappedRoot = MapPath(Root);
            if (filePath.StartsWith(mappedRoot) == false)
                filePath = MapPath(filePath);

            // yes we can (see above)
            //// don't trust what we get, it may contain relative segments
            //filePath = Path.GetFullPath(filePath);

            foreach (var dir in validDirs)
            {
                var validDir = dir;
                if (validDir.StartsWith(mappedRoot) == false)
                    validDir = MapPath(validDir);

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

        /// <summary>
        /// Returns the path to the root of the application, by getting the path to where the assembly where this
        /// method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
        /// even if the assembly is in a /bin/debug or /bin/release folder
        /// </summary>
        /// <returns></returns>
        public string GetRootDirectorySafe()
        {
            if (String.IsNullOrEmpty(_rootDir) == false)
            {
                return _rootDir;
            }

            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var baseDirectory = Path.GetDirectoryName(path);
            if (String.IsNullOrEmpty(baseDirectory))
                throw new Exception("No root directory could be resolved. Please ensure that your Umbraco solution is correctly configured.");

            _rootDir = baseDirectory.Contains("bin")
                           ? baseDirectory.Substring(0, baseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase) - 1)
                           : baseDirectory;

            return _rootDir;
        }

        public string GetRootDirectoryBinFolder()
        {
            string binFolder = String.Empty;
            if (String.IsNullOrEmpty(_rootDir))
            {
                binFolder = Assembly.GetExecutingAssembly().GetAssemblyFile().Directory.FullName;
                return binFolder;
            }

            binFolder = Path.Combine(GetRootDirectorySafe(), "bin");

            // do this all the time (no #if DEBUG) because Umbraco release
            // can be used in tests by an app (eg Deploy) being debugged
            var debugFolder = Path.Combine(binFolder, "debug");
            if (Directory.Exists(debugFolder))
                return debugFolder;

            var releaseFolder = Path.Combine(binFolder, "release");
            if (Directory.Exists(releaseFolder))
                return releaseFolder;

            if (Directory.Exists(binFolder))
                return binFolder;

            return _rootDir;
        }

        /// <summary>
        /// Allows you to overwrite RootDirectory, which would otherwise be resolved
        /// automatically upon application start.
        /// </summary>
        /// <remarks>The supplied path should be the absolute path to the root of the umbraco site.</remarks>
        /// <param name="rootPath"></param>
        public void SetRootDirectory(string rootPath)
        {
            _rootDir = rootPath;
        }

        /// <summary>
        /// Check to see if filename passed has any special chars in it and strips them to create a safe filename.  Used to overcome an issue when Umbraco is used in IE in an intranet environment.
        /// </summary>
        /// <param name="filePath">The filename passed to the file handler from the upload field.</param>
        /// <returns>A safe filename without any path specific chars.</returns>
        public string SafeFileName(string filePath)
        {
            // use string extensions
            return filePath.ToSafeFileName();
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
                var rootDirectory = GetRootDirectorySafe();
                var relativePath = path.ToLowerInvariant().Replace(rootDirectory.ToLowerInvariant(), string.Empty);
                path = relativePath;
            }

            return path.EnsurePathIsApplicationRootPrefixed();
        }


        public string Media => ReturnPath("umbracoMediaPath", "~/media");

        public string Scripts => ReturnPath("umbracoScriptsPath", "~/scripts");

        public string Css => ReturnPath("umbracoCssPath", "~/css");

        public string Umbraco => ReturnPath("umbracoPath", "~/umbraco");

        private string _root;

        /// <summary>
        /// Gets the root path of the application
        /// </summary>
        public string Root
        {
            get
            {
                if (_root != null) return _root;

                var appPath = HttpRuntime.AppDomainAppVirtualPath;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (appPath == null || appPath == "/") appPath = string.Empty;

                _root = appPath;

                return _root;
            }
            //Only required for unit tests
            set => _root = value;
        }
    }
}
