using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using umbraco.IO;
using umbraco.businesslogic;

namespace umbraco.presentation
{
    public class EnsureSystemPathsApplicationStartupHandler : ApplicationStartupHandler
    {
        public EnsureSystemPathsApplicationStartupHandler()
        {
            EnsurePathExists(SystemDirectories.Css);
            EnsurePathExists(SystemDirectories.Data);
            EnsurePathExists(SystemDirectories.MacroScripts);
            EnsurePathExists(SystemDirectories.Masterpages);
            EnsurePathExists(SystemDirectories.Media);
            EnsurePathExists(SystemDirectories.Scripts);
            EnsurePathExists(SystemDirectories.Usercontrols);
            EnsurePathExists(SystemDirectories.Xslt);
        }

        public void EnsurePathExists(string path)
        {
            var absolutePath = IOHelper.MapPath(path);
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
        }
    }
}