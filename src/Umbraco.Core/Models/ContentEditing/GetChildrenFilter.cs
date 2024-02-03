namespace Umbraco.Cms.Core.Models.ContentEditing
{
    /// <summary>
    /// Represents a filter for fetching child content items with optional sorting and pagination.
    /// </summary>
    public class GetChildrenFilter
    {
        /// <summary>
        /// The unique identifier for the parent content item whose children are to be retrieved.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A comma-separated list of property aliases to include in the response. Null or empty to include all.
        /// </summary>
        public string? IncludeProperties { get; set; }

        /// <summary>
        /// The page number for pagination, starting at 1. A value of 0 indicates no pagination.
        /// </summary>
        public int PageNumber { get; set; } = 0;

        /// <summary>
        /// The number of items per page for pagination. A value of 0 indicates no pagination.
        /// </summary>
        public int PageSize { get; set; } = 0;

        /// <summary>
        /// The property name by which to order the child items. Defaults to "SortOrder".
        /// </summary>
        public string OrderBy { get; set; } = "SortOrder";

        /// <summary>
        /// The direction in which to order the child items (Ascending or Descending).
        /// </summary>
        public Direction OrderDirection { get; set; } = Direction.Ascending;

        /// <summary>
        /// Indicates whether the ordering should be applied to system fields. True to use system fields.
        /// </summary>
        public bool OrderBySystemField { get; set; } = true;

        /// <summary>
        /// A filter string to apply when retrieving child items. Could be used to match names, IDs, or other properties.
        /// </summary>
        public string Filter { get; set; } = "";

        /// <summary>
        /// The culture code (ISO) to filter content by language. Empty string indicates no culture filtering.
        /// Note: Originally named CultureName, but it actually expects a culture ISO code.
        /// </summary>
        public string CultureName { get; set; } = "";  // TODO: it's not a NAME it's the ISO CODE
    }
}
