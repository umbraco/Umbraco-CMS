namespace Umbraco.Core
{
    public static partial class Constants
    {
        public static class SystemDirectories
        {
            public const string Bin = "~/bin";

            public const string Config = "~/config";

            public const string Data = "~/Umbraco/Data";

            public const string TempData = Data + "/TEMP";

            public const string TempFileUploads = TempData + "/FileUploads";

            public const string TempImageUploads = TempFileUploads + "/rte";

            public const string Install = "~/install";

            public const string AppPlugins = "/App_Plugins";

            public const string MvcViews = "~/Views";

            public const string PartialViews = MvcViews + "/Partials/";

            public const string MacroPartials = MvcViews + "/MacroPartials/";

            public const string Packages = Data + "/packages";

            public const string Preview = Data + "/preview";

            // TODO: This doesn't seem right?
            public const string LogFiles= "~/Logs";
        }
    }
}
