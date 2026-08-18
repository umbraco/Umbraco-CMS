namespace Umbraco.Cms.Search.Provider.Examine.Configuration;

/// <summary>
/// Configures which property values are indexed as searchable Lucene fields, and how.
/// </summary>
public sealed class FieldOptions
{
    /// <summary>
    /// Gets or sets the fields to configure for indexing.
    /// </summary>
    public required Field[] Fields { get; set; } = [];

    /// <summary>
    /// Describes how a single property is indexed as a Lucene field.
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Gets the alias of the property to index.
        /// </summary>
        public required string PropertyName { get; init; }

        /// <summary>
        /// Gets the type of values to index the property as.
        /// </summary>
        public required FieldValues FieldValues { get; init; }

        /// <summary>
        /// Gets a value indicating whether the field can be used for sorting.
        /// </summary>
        public bool Sortable { get; init; }

        /// <summary>
        /// Gets a value indicating whether the field can be used for faceting.
        /// </summary>
        public bool Facetable { get; init; }

        /// <summary>
        /// Gets the segments the field should additionally be indexed for, on top of the invariant/culture-only field.
        /// </summary>
        public string[] Segments { get; init; } = [];
    }
}
