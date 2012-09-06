using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using System.Web;
using System.IO;

namespace Umbraco.Core.IO
{
    //all paths has a starting but no trailing /
	internal class SystemDirectories
    {
        public static string Bin
        {
            get
            {
                return IOHelper.ReturnPath("umbracoBinDirectory", "~/bin");
            }
        }

        public static string Base
        {
            get
            {
                return IOHelper.ReturnPath("umbracoBaseDirectory", "~/base");
            }
        }

        public static string Config
        {
            get
            {
                return IOHelper.ReturnPath("umbracoConfigDirectory", "~/config");
            }
        }
                
        public static string Css
        {
            get
            {
                return IOHelper.ReturnPath("umbracoCssDirectory", "~/css");
            }
        }

        public static string Data
        {
            get
            {
                return IOHelper.ReturnPath("umbracoStorageDirectory", "~/App_Data");
            }
        }

        public static string Install
        {
            get
            {
                return IOHelper.ReturnPath("umbracoInstallPath", "~/install");
            }
        }

        public static string Masterpages
        {
            get
            {
                return IOHelper.ReturnPath("umbracoMasterPagesPath", "~/masterpages");
            }
        }

       
        public static string Media
        {
            get
            {
                return IOHelper.ReturnPath("umbracoMediaPath", "~/media");
            }
        }

        [Obsolete("Please use MacroScripts instead!", true)]
        public static string Python
        {
            get
            {
                return MacroScripts;
            }
        }

        public static string MacroScripts
        {
            get
            {
                // for legacy we test for the python path first, but else we use the new default location
                string tempPath = IOHelper.ReturnPath("umbracoPythonPath", "") == String.Empty
                                      ? IOHelper.ReturnPath("umbracoMacroScriptPath", "~/macroScripts")
                                      : IOHelper.ReturnPath("umbracoPythonPath", "~/python");
                return tempPath;
            }
        }

        public static string Scripts
        {
            get
            {
                return IOHelper.ReturnPath("umbracoScriptsPath", "~/scripts");
            }
        }

        public static string Umbraco
        {
            get
            {
                return IOHelper.ReturnPath("umbracoPath", "~/umbraco");
            }
        }

        public static string UmbracoClient
        {
            get
            {
                return IOHelper.ReturnPath("umbracoClientPath", "~/umbraco_client");
            }
        }

        public static string UserControls
        {
            get
            {
                return IOHelper.ReturnPath("umbracoUsercontrolsPath", "~/usercontrols");
            }
        }

        public static string WebServices
        {
            get
            {
                return IOHelper.ReturnPath("umbracoWebservicesPath", "~/umbraco/webservices");
            }
        }

        public static string Xslt
        {
            get {
                return IOHelper.ReturnPath("umbracoXsltPath", "~/xslt");
            }
        }

        public static string Packages
        {
            get
            {
                //by default the packages folder should exist in the data folder
                return IOHelper.ReturnPath("umbracoPackagesPath", Data + IOHelper.DirSepChar + "packages");
            }
        }

        public static string Preview
        {
            get
            {
                //by default the packages folder should exist in the data folder
                return IOHelper.ReturnPath("umbracoPreviewPath", Data + IOHelper.DirSepChar + "preview");
            }
        }

        public static string Root
        {
            get
            {
                string appPath = HttpRuntime.AppDomainAppVirtualPath ?? string.Empty;
                if (appPath == "/")
                    appPath = string.Empty;

                return appPath;
            }
        }
    }


    
}
