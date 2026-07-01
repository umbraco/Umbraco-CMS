namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents an Index
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class IndexAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAttribute"/> class using the specified index type.
    /// </summary>
    /// <param name="indexType">The index type to apply.</param>
    public IndexAttribute(IndexTypes indexType) => IndexType = indexType;

    /// <summary>
    ///     Gets or sets the name of the Index
    /// </summary>
    /// <remarks>
    ///     Overrides default naming of indexes:
    ///     IX_tableName
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the type of index to create
    /// </summary>
    public IndexTypes IndexType { get; }

    /// <summary>
    ///     Gets or sets the column name(s) for the current index
    /// </summary>
    public string? ForColumns { get; set; }

    /// <summary>
    ///     Gets or sets the column name(s) for the columns to include in the index
    /// </summary>
    public string? IncludeColumns { get; set; }
}
