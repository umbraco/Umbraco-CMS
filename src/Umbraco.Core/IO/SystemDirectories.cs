using System;
using System.Web;

namespace Umbraco.Core.IO
{
    //all paths has a starting but no trailing /
    public class SystemDirectories
    {
        public static string Bin => "~/bin";

        public static string Config => IOHelper.ReturnPath("umbracoConfigDirectory", "~/config");

        public static string Css => IOHelper.ReturnPath("umbracoCssDirectory", "~/css");

        public static string Data => IOHelper.ReturnPath("umbracoStorageDirectory", "~/App_Data");

        public static string Install => IOHelper.ReturnPath("umbracoInstallPath", "~/install");

        //TODO: Consider removing this
        public static string Masterpages => IOHelper.ReturnPath("umbracoMasterPagesPath", "~/masterpages");

        //NOTE: this is not configurable and shouldn't need to be
        public static string AppCode => "~/App_Code";

        //NOTE: this is not configurable and shouldn't need to be
        public static string AppPlugins => "~/App_Plugins";

        //NOTE: this is not configurable and shouldn't need to be
        public static string MvcViews => "~/Views";

        public static string PartialViews => MvcViews + "/Partials/";

        public static string MacroPartials => MvcViews + "/MacroPartials/";

        public static string Media => IOHelper.ReturnPath("umbracoMediaPath", "~/media");

        public static string Scripts => IOHelper.ReturnPath("umbracoScriptsPath", "~/scripts");

        public static string StyleSheets => IOHelper.ReturnPath("umbracoStylesheetsPath", "~/css");

        public static string Umbraco => IOHelper.ReturnPath("umbracoPath", "~/umbraco");

        //TODO: Consider removing this
        public static string UserControls => IOHelper.ReturnPath("umbracoUsercontrolsPath", "~/usercontrols");

        public static string WebServices => IOHelper.ReturnPath("umbracoWebservicesPath", Umbraco.EnsureEndsWith("/") + "webservices");

        //by default the packages folder should exist in the data folder
        public static string Packages => IOHelper.ReturnPath("umbracoPackagesPath", Data + IOHelper.DirSepChar + "packages");

        public static string Preview => IOHelper.ReturnPath("umbracoPreviewPath", Data + IOHelper.DirSepChar + "preview");

        public static string JavaScriptLibrary => IOHelper.ReturnPath("umbracoJavaScriptLibraryPath", Umbraco + IOHelper.DirSepChar + "lib");

        private static string _root;

        /// <summary>
        /// Gets the root path of the application
        /// </summary>
        public static string Root
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
            internal set => _root = value;
        }
    }
}
