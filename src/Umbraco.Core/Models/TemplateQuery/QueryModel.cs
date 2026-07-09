namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents a query model for building template queries with filters and sorting.
/// </summary>
public class QueryModel
{
    /// <summary>
    ///     Gets or sets the content type to filter by.
    /// </summary>
    public ContentTypeModel? ContentType { get; set; }

    /// <summary>
    ///     Gets or sets the source node for the query.
    /// </summary>
    public SourceModel? Source { get; set; }

    /// <summary>
    ///     Gets or sets the collection of filter conditions.
    /// </summary>
    public IEnumerable<QueryCondition>? Filters { get; set; }

    /// <summary>
    ///     Gets or sets the sort expression for ordering results.
    /// </summary>
    public SortExpression? Sort { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of items to return.
    /// </summary>
    public int Take { get; set; }
}
