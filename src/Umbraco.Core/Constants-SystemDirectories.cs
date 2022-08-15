namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class SystemDirectories
    {
        /// <summary>
        ///     The aspnet bin folder
        /// </summary>
        public const string Bin = "~/bin";

        // TODO: Shouldn't this exist underneath /Umbraco in the content root?
        public const string Config = "~/config";

        /// <summary>
        ///     The Umbraco folder that exists at the content root.
        /// </summary>
        /// <remarks>
        ///     This is not the same as the Umbraco web folder which is configurable for serving front-end files.
        /// </remarks>
        public const string Umbraco = "~/umbraco";

        /// <summary>
        ///     The Umbraco data folder in the content root.
        /// </summary>
        public const string Data = Umbraco + "/Data";

        /// <summary>
        ///     The Umbraco licenses folder in the content root.
        /// </summary>
        public const string Licenses = Umbraco + "/Licenses";

        /// <summary>
        ///     The Umbraco temp data folder in the content root.
        /// </summary>
        public const string TempData = Data + "/TEMP";

        public const string TempFileUploads = TempData + "/FileUploads";

        public const string TempImageUploads = TempFileUploads + "/rte";

        public const string Install = "~/install";

        public const string AppPlugins = "/App_Plugins";

        public const string PluginIcons = "/backoffice/icons";

        public const string MvcViews = "~/Views";

        public const string PartialViews = MvcViews + "/Partials/";

        public const string MacroPartials = MvcViews + "/MacroPartials/";

        public const string Packages = Data + "/packages";

        public const string CreatedPackages = Data + "/CreatedPackages";

        public const string Preview = Data + "/preview";

        /// <summary>
        ///     The default folder where Umbraco log files are stored
        /// </summary>
        public const string LogFiles = Umbraco + "/Logs";

        [Obsolete("Use PluginIcons instead")]
        public static string AppPluginIcons => "/Backoffice/Icons";
    }
}
