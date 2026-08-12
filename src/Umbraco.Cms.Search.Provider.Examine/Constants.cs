namespace Umbraco.Cms.Search.Provider.Examine;

/// <summary>
/// Constants specific to the Examine search provider (field name suffixes, system field names, API identifiers).
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Identifiers for this provider's own Management API.
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// The API name used to map this provider's own Management API endpoints.
        /// </summary>
        public const string Name = "search-examine-provider";
    }

    /// <summary>
    /// Identifiers for this search provider.
    /// </summary>
    public static class Provider
    {
        /// <summary>
        /// The name of this search provider.
        /// </summary>
        public const string Name = Api.Name;
    }

    /// <summary>
    /// Values used to represent variation (culture/segment) state.
    /// </summary>
    public static class Variance
    {
        /// <summary>
        /// The value used for the culture field of an invariant document.
        /// </summary>
        public const string Invariant = "none";
    }

    /// <summary>
    /// Field name suffixes identifying the type of values indexed for a field.
    /// </summary>
    public static class FieldValues
    {
        /// <summary>
        /// The suffix for a field holding integer values.
        /// </summary>
        public const string Integers = "integers";

        /// <summary>
        /// The suffix for a field holding decimal values.
        /// </summary>
        public const string Decimals = "decimals";

        /// <summary>
        /// The suffix for a field holding date/time values.
        /// </summary>
        public const string DateTimeOffsets = "datetimeoffsets";

        /// <summary>
        /// The suffix for a field holding exact-match keyword values.
        /// </summary>
        public const string Keywords = "keywords";

        /// <summary>
        /// The suffix for a field holding full-text values.
        /// </summary>
        public const string Texts = "texts";

        /// <summary>
        /// The suffix for a field holding full-text values at relevance level 1 (highest).
        /// </summary>
        public const string TextsR1 = "textsr1";

        /// <summary>
        /// The suffix for a field holding full-text values at relevance level 2.
        /// </summary>
        public const string TextsR2 = "textsr2";

        /// <summary>
        /// The suffix for a field holding full-text values at relevance level 3 (lowest).
        /// </summary>
        public const string TextsR3 = "textsr3";
    }

    /// <summary>
    /// Names of this provider's own system fields (distinct from <see cref="Umbraco.Cms.Search.Core.Constants.FieldNames"/>).
    /// </summary>
    public static class SystemFields
    {
        private const string Prefix = "Sys_";

        /// <summary>
        /// The field name for a document's public-access protection metadata.
        /// </summary>
        public const string Protection = $"{Prefix}Protection";

        /// <summary>
        /// The field name for a document's culture.
        /// </summary>
        public const string Culture = $"{Prefix}Culture";

        /// <summary>
        /// The field name for aggregated full-text values across all relevance levels.
        /// </summary>
        public const string AggregatedTexts = $"{Prefix}aggregated_texts";

        /// <summary>
        /// The field name for aggregated full-text values at relevance level 1 (highest).
        /// </summary>
        public const string AggregatedTextsR1 = $"{Prefix}aggregated_textsr1";

        /// <summary>
        /// The field name for aggregated full-text values at relevance level 2.
        /// </summary>
        public const string AggregatedTextsR2 = $"{Prefix}aggregated_textsr2";

        /// <summary>
        /// The field name for aggregated full-text values at relevance level 3 (lowest).
        /// </summary>
        public const string AggregatedTextsR3 = $"{Prefix}aggregated_textsr3";
    }
}
