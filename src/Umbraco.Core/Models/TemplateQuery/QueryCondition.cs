namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents a filter condition for a template query.
/// </summary>
public class QueryCondition
{
    /// <summary>
    ///     Gets or sets the property to filter on.
    /// </summary>
    public PropertyModel Property { get; set; } = new();

    /// <summary>
    ///     Gets or sets the operator term defining the comparison operation.
    /// </summary>
    public OperatorTerm Term { get; set; } = new();

    /// <summary>
    ///     Gets or sets the constraint value to compare against.
    /// </summary>
    public string ConstraintValue { get; set; } = string.Empty;
}
