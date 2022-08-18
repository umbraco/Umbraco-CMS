namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines the identifiers for Umbraco system nodes.
    /// </summary>
    public static class System
    {
        /// <summary>
        ///     The integer identifier for global system root node.
        /// </summary>
        public const int Root = -1;

        /// <summary>
        ///     The string identifier for global system root node.
        /// </summary>
        /// <remarks>Use this instead of re-creating the string everywhere.</remarks>
        public const string RootString = "-1";

        /// <summary>
        ///     The integer identifier for content's recycle bin.
        /// </summary>
        public const int RecycleBinContent = -20;

        /// <summary>
        ///     The string identifier for content's recycle bin.
        /// </summary>
        /// <remarks>Use this instead of re-creating the string everywhere.</remarks>
        public const string RecycleBinContentString = "-20";

        /// <summary>
        ///     The string path prefix of the content's recycle bin.
        /// </summary>
        /// <remarks>
        ///     <para>Everything that is in the content recycle bin, has a path that starts with the prefix.</para>
        ///     <para>Use this instead of re-creating the string everywhere.</para>
        /// </remarks>
        public const string RecycleBinContentPathPrefix = "-1,-20,";

        /// <summary>
        ///     The integer identifier for media's recycle bin.
        /// </summary>
        public const int RecycleBinMedia = -21;

        /// <summary>
        ///     The string identifier for media's recycle bin.
        /// </summary>
        /// <remarks>Use this instead of re-creating the string everywhere.</remarks>
        public const string RecycleBinMediaString = "-21";

        /// <summary>
        ///     The string path prefix of the media's recycle bin.
        /// </summary>
        /// <remarks>
        ///     <para>Everything that is in the media recycle bin, has a path that starts with the prefix.</para>
        ///     <para>Use this instead of re-creating the string everywhere.</para>
        /// </remarks>
        public const string RecycleBinMediaPathPrefix = "-1,-21,";

        public const int DefaultLabelDataTypeId = -92;

        public const string UmbracoDefaultDatabaseName = "Umbraco";

        public const string UmbracoConnectionName = "umbracoDbDSN";
        public const string DefaultUmbracoPath = "~/umbraco";
    }
}
