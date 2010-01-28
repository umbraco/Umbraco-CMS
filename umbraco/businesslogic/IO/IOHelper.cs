using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;
using umbraco.BusinessLogic;

namespace umbraco.IO
{
    public class IOHelper
    {
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
            
            if(virtualPath.StartsWith("/") && !virtualPath.StartsWith(SystemDirectories.Root))
                retval = SystemDirectories.Root + "/" + virtualPath.TrimStart('/');

            return retval;
        }

        //Replaces tildes with the root dir
        public static string ResolveUrl(string virtualPath)
        {
            if (virtualPath.StartsWith("~"))
                return virtualPath.Replace("~", SystemDirectories.Root).Replace("//","/");
            else
                return VirtualPathUtility.ToAbsolute(virtualPath, SystemDirectories.Root);
        }

        public static string MapPath(string path, bool useHttpContext)
        {
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
                string _root = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath.TrimEnd(IOHelper.DirSepChar);
                string _path = path.TrimStart('~','/').Replace('/', IOHelper.DirSepChar);

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

            if ( string.IsNullOrEmpty(retval) )
                retval = standardPath;

            return retval.TrimEnd('/');
        }

        
        public static string returnPath(string settingsKey, string standardPath)
        {
            return returnPath(settingsKey, standardPath, false);

        }

    }
}
