namespace Umbraco.Core
{
    /// <summary>
    /// Defines constants.
    /// </summary>
    public static partial class Constants
    {
        /// <summary>
        /// Defines constants for composition.
        /// </summary>
        public static class Composing
        {
            /// <summary>
            /// Defines file system names.
            /// </summary>
            public static class FileSystems
            {
                public const string ScriptFileSystem = "ScriptFileSystem";
                public const string PartialViewFileSystem = "PartialViewFileSystem";
                public const string PartialViewMacroFileSystem = "PartialViewMacroFileSystem";
                public const string StylesheetFileSystem = "StylesheetFileSystem";
                public const string MasterpageFileSystem = "MasterpageFileSystem";
                public const string ViewFileSystem = "ViewFileSystem";
                public const string JavascriptLibraryFileSystem = "JavascriptLibraryFileSystem";
            }
        }
    }
}
