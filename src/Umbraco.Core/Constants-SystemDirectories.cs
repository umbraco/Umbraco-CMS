namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for Umbraco system directory paths.
    /// </summary>
    public static class SystemDirectories
    {
        /// <summary>
        ///     The aspnet bin folder
        /// </summary>
        public const string Bin = "~/bin";

        // TODO: Shouldn't this exist underneath /Umbraco in the content root?

        /// <summary>
        ///     The configuration folder path.
        /// </summary>
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

        /// <summary>
        ///     The temporary file uploads folder path.
        /// </summary>
        public const string TempFileUploads = TempData + "/FileUploads";

        /// <summary>
        ///     The install folder path.
        /// </summary>
        public const string Install = "~/install";

        /// <summary>
        ///     The App_Plugins folder path for custom packages.
        /// </summary>
        public const string AppPlugins = "/App_Plugins";

        /// <summary>
        ///     The backoffice path.
        /// </summary>
        public const string BackOfficePath = "/umbraco/backoffice";

        /// <summary>
        ///     The MVC Views folder path.
        /// </summary>
        public const string MvcViews = "~/Views";

        /// <summary>
        ///     The partial views folder path.
        /// </summary>
        public const string PartialViews = MvcViews + "/Partials/";

        /// <summary>
        ///     The packages folder path for installed packages.
        /// </summary>
        public const string Packages = Data + "/packages";

        /// <summary>
        ///     The created packages folder path for exported packages.
        /// </summary>
        public const string CreatedPackages = Data + "/CreatedPackages";

        /// <summary>
        ///     The preview folder path for content preview data.
        /// </summary>
        public const string Preview = Data + "/preview";

        /// <summary>
        ///     The default folder where Umbraco log files are stored
        /// </summary>
        [Obsolete("Use LoggingSettings.GetLoggingDirectory() instead, will be removed in Umbraco 13.")]
        public const string LogFiles = Umbraco + "/Logs";
    }
}
