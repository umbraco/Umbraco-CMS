namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    /// Defines the Umbraco system constants.
    /// </summary>
    public static class System
    {
        /// <summary>
        /// The integer identifier for global system root node.
        /// </summary>
        public const int Root = -1;

        /// <summary>
        /// The string identifier for global system root node.
        /// </summary>
        /// <remarks>
        /// Use this instead of re-creating the string everywhere.
        /// </remarks>
        public const string RootString = "-1";

        /// <summary>
        /// The GUID identifier for global system root node.
        /// </summary>
        public static readonly Guid? RootKey = null;

        /// <summary>
        /// The integer identifier for content's recycle bin.
        /// </summary>
        public const int RecycleBinContent = -20;

        /// <summary>
        /// The string identifier for content's recycle bin.
        /// </summary>
        /// <remarks>
        /// Use this instead of re-creating the string everywhere.
        /// </remarks>
        public const string RecycleBinContentString = "-20";

        /// <summary>
        /// The GUID identifier for content's recycle bin.
        /// </summary>
        public static readonly Guid RecycleBinContentKey = new("0F582A79-1E41-4CF0-BFA0-76340651891A");

        /// <summary>
        /// The string path prefix of the content's recycle bin.
        /// </summary>
        /// <remarks>
        /// <para>Everything that is in the content recycle bin, has a path that starts with the prefix.</para>
        /// <para>Use this instead of re-creating the string everywhere.</para>
        /// </remarks>
        public const string RecycleBinContentPathPrefix = "-1,-20,";

        /// <summary>
        /// The integer identifier for media's recycle bin.
        /// </summary>
        public const int RecycleBinMedia = -21;

        /// <summary>
        /// The string identifier for media's recycle bin.
        /// </summary>
        /// <remarks>
        /// Use this instead of re-creating the string everywhere.
        /// </remarks>
        public const string RecycleBinMediaString = "-21";

        /// <summary>
        /// The GUID identifier for media's recycle bin.
        /// </summary>
        public static readonly Guid RecycleBinMediaKey = new("BF7C7CBC-952F-4518-97A2-69E9C7B33842");

        /// <summary>
        /// The string path prefix of the media's recycle bin.
        /// </summary>
        /// <remarks>
        /// <para>Everything that is in the media recycle bin, has a path that starts with the prefix.</para>
        /// <para>Use this instead of re-creating the string everywhere.</para>
        /// </remarks>
        public const string RecycleBinMediaPathPrefix = "-1,-21,";

        /// <summary>
        /// The default label data type identifier.
        /// </summary>
        public const int DefaultLabelDataTypeId = -92;

        /// <summary>
        /// The default Umbraco database name.
        /// </summary>
        public const string UmbracoDefaultDatabaseName = "Umbraco";

        /// <summary>
        /// The Umbraco connection name.
        /// </summary>
        public const string UmbracoConnectionName = "umbracoDbDSN";

        /// <summary>
        /// The Umbraco path segment.
        /// </summary>
        public const string UmbracoPathSegment = "umbraco";

        /// <summary>
        /// The default Umbraco virtual path.
        /// </summary>
        public const string DefaultUmbracoPath = "~/" + UmbracoPathSegment;

        /// <summary>
        /// The application domain data name for <c>DataDirectory</c>.
        /// </summary>
        public const string DataDirectoryName = "DataDirectory";

        /// <summary>
        /// The <c>DataDirectory</c> placeholder used to resolve paths in connection strings.
        /// </summary>
        public const string DataDirectoryPlaceholder = "|DataDirectory|";

        public const string InvariantCulture = "*";
    }
}
