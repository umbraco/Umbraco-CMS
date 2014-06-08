namespace Umbraco.Web.Editors
{
    public class QueryCondition : IQueryCondition
    {
        
        public string FieldName { get; set; }
        public IOperathorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }


    internal static class QueryConditionExtensions
    {
        public static string MakeBinaryOperation(this IQueryCondition condition, string operation)
        {
            return string.Format("{0}{1}{2}", condition.FieldName, operation, condition.ConstraintValue);
        }

    }
}