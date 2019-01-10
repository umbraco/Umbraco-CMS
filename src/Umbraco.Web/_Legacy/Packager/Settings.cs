using System;
using System.IO;
using Umbraco.Core.IO;

namespace Umbraco.Web._Legacy.Packager
{
    public class Settings
    {
        public static string InstalledPackagesSettings => SystemDirectories.Packages + IOHelper.DirSepChar + "installedPackages.config";

    }

}
