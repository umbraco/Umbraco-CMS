using System.Web;
using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    //all paths has a starting but no trailing /
    public class SystemDirectories : ISystemDirectories
    {
        public string Bin => "~/bin";

        public string Config => "~/config";

        public string Data => "~/App_Data";

        public string TempData => Data + "/TEMP";

        public string TempFileUploads => TempData + "/FileUploads";

        public string TempImageUploads => TempFileUploads + "/rte";

        public string Install => "~/install";

        public string AppCode => "~/App_Code";

        public string AppPlugins => "~/App_Plugins";

        public string MvcViews => "~/Views";

        public string PartialViews => MvcViews + "/Partials/";

        public string MacroPartials => MvcViews + "/MacroPartials/";

        public string Media => Current.IOHelper.ReturnPath("umbracoMediaPath", "~/media");

        public string Scripts => Current.IOHelper.ReturnPath("umbracoScriptsPath", "~/scripts");

        public string Css => Current.IOHelper.ReturnPath("umbracoCssPath", "~/css");

        public string Umbraco => Current.IOHelper.ReturnPath("umbracoPath", "~/umbraco");

        public string Packages => Data + "/packages";

        public string Preview => Data + "/preview";

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
