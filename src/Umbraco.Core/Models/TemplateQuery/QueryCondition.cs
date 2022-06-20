namespace Umbraco.Cms.Core.Models.TemplateQuery;

public class QueryCondition
{
    public PropertyModel Property { get; set; } = new();

    public OperatorTerm Term { get; set; } = new();

    public string ConstraintValue { get; set; } = string.Empty;
}
