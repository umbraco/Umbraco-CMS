namespace Umbraco.Web.Editors
{
    using Umbraco.Web.Editors.TemplateQuery;

    public class QueryCondition
    {
        
        public PropertyModel Property { get; set; }
        public OperathorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }


    internal static class QueryConditionExtensions
    {
        public static string MakeBinaryOperation(this QueryCondition condition, string operation)
        {
            return string.Format("{0}{1}{2}", condition.Property.Name, operation, condition.ConstraintValue);
        }

    }
}