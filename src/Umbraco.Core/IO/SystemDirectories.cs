using System.Web;

namespace Umbraco.Core.IO
{
    //all paths has a starting but no trailing /
    public class SystemDirectories
    {
        public static string Bin => "~/bin";

        public static string Config => "~/config";

        public static string Data => "~/App_Data";

        public static string LogFiles => Data + "/Logs";

        public static string TempData => Data + "/TEMP";

        public static string TempFileUploads => TempData + "/FileUploads";

        public static string TempImageUploads => TempFileUploads + "/rte";

        public static string Install => "~/install";

        public static string AppCode => "~/App_Code";

        public static string AppPlugins => "~/App_Plugins";

        public static string AppPluginIcons => "/Backoffice/Icons";

        public static string MvcViews => "~/Views";

        public static string PartialViews => MvcViews + "/Partials/";

        public static string MacroPartials => MvcViews + "/MacroPartials/";

        public static string Media => IOHelper.ReturnPath(Constants.AppSettings.MediaPath, "~/media");

        public static string Scripts => IOHelper.ReturnPath(Constants.AppSettings.ScriptsPath, "~/scripts");

        public static string Css => IOHelper.ReturnPath(Constants.AppSettings.CssPath, "~/css");

        public static string Umbraco => IOHelper.ReturnPath(Constants.AppSettings.Path, "~/umbraco");

        public static string Packages => Data + "/packages";

        public static string Preview => Data + "/preview";

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
