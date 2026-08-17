namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for Umbraco Search index aliases.
    /// </summary>
    public static class IndexAliases
    {
        private const string IndexPrefix = "Umb_";

        /// <summary>
        ///     The alias of the published content index, used as the default index for template searches via
        ///     <see cref="IPublishedContentQuery" />.
        /// </summary>
        public const string PublishedContent = $"{IndexPrefix}PublishedContent";

        /// <summary>
        ///     The alias of the draft content index.
        /// </summary>
        public const string DraftContent = $"{IndexPrefix}Content";

        /// <summary>
        ///     The alias of the draft media index.
        /// </summary>
        public const string DraftMedia = $"{IndexPrefix}Media";

        /// <summary>
        ///     The alias of the draft members index.
        /// </summary>
        public const string DraftMembers = $"{IndexPrefix}Members";
    }
}
