using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;
using umbraco.BusinessLogic;
using System.Text.RegularExpressions;

namespace umbraco.IO
{
    public static class IOHelper
    {
        private static string m_rootDir = "";

        public static char DirSepChar
        {
            get
            {
                return Path.DirectorySeparatorChar;
            }
        }

        //helper to try and match the old path to a new virtual one
        public static string FindFile(string virtualPath)
        {
            string retval = virtualPath;

            if (virtualPath.StartsWith("~"))
                retval = virtualPath.Replace("~", SystemDirectories.Root);

            if (virtualPath.StartsWith("/") && !virtualPath.StartsWith(SystemDirectories.Root))
                retval = SystemDirectories.Root + "/" + virtualPath.TrimStart('/');

            return retval;
        }

        //Replaces tildes with the root dir
        public static string ResolveUrl(string virtualPath)
        {
            if (virtualPath.StartsWith("~"))
                return virtualPath.Replace("~", SystemDirectories.Root).Replace("//", "/");
            else
                return VirtualPathUtility.ToAbsolute(virtualPath, SystemDirectories.Root);
        }

        public static string ResolveUrlsFromTextString(string text)
        {
            // find all relative urls (ie. urls that contain ~)
            string pattern = "(\\S+)=[\"']?(\\W?\\~(?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?";
            MatchCollection tags =
                Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match tag in tags)
            {
                string url = "";
                if (tag.Groups[2].Success)
                    url = tag.Groups[2].Value;

                // The richtext editor inserts a slash in front of the url. That's why we need this little fix
//                if (url.StartsWith("/"))
//                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
//                else
                if (!String.IsNullOrEmpty(url))
                {
                    string resolvedUrl = (url.Substring(0,1) == "/") ? ResolveUrl(url.Substring(1)) : ResolveUrl(url);
                    text = text.Replace(url, resolvedUrl);
                }

            }

            return text;
        }

        public static string MapPath(string path, bool useHttpContext)
        {
            // Check if the path is already mapped
            if (path.Length >= 2 && path[1] == Path.VolumeSeparatorChar)
                return path;

            if (useHttpContext)
            {
                //string retval;
                if (!string.IsNullOrEmpty(path) && (path.StartsWith("~") || path.StartsWith(SystemDirectories.Root)))
                    return System.Web.Hosting.HostingEnvironment.MapPath(path);
                else
                    return System.Web.Hosting.HostingEnvironment.MapPath("~/" + path.TrimStart('/'));
            }
            else
            {
                string _root = (!String.IsNullOrEmpty(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath)) ? System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath.TrimEnd(IOHelper.DirSepChar) : getRootDirectorySafe();

                string _path = path.TrimStart('~', '/').Replace('/', IOHelper.DirSepChar);

                string retval = _root + IOHelper.DirSepChar.ToString() + _path;

                return retval;
            }
        }

        public static string MapPath(string path)
        {
            return MapPath(path, true);
        }

        //use a tilde character instead of the complete path
        public static string returnPath(string settingsKey, string standardPath, bool useTilde)
        {
            string retval = ConfigurationManager.AppSettings[settingsKey];

            if (string.IsNullOrEmpty(retval))
                retval = standardPath;

            return retval.TrimEnd('/');
        }


        public static string returnPath(string settingsKey, string standardPath)
        {
            return returnPath(settingsKey, standardPath, false);

        }

        /// <summary>
        /// Returns the path to the root of the application, by getting the path to where the assembly where this
        /// method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
        /// even if the assembly is in a /bin/debug or /bin/release folder
        /// </summary>
        /// <returns></returns>
        private static string getRootDirectorySafe()
        {
            if (!String.IsNullOrEmpty(m_rootDir))
            {
                return m_rootDir;
            }

            string baseDirectory =
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            m_rootDir = baseDirectory.Substring(0, baseDirectory.LastIndexOf("bin") - 1);

            return m_rootDir;

        }

    }
}
