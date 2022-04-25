namespace Umbraco.Cms.Core.Models.TemplateQuery
{
    public class QueryCondition
    {
        public PropertyModel Property { get; set; } = new PropertyModel();
        public OperatorTerm Term { get; set; } = new OperatorTerm();
        public string ConstraintValue { get; set; } = string.Empty;
    }
}
