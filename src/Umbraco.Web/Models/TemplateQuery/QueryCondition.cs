namespace Umbraco.Web.Models.TemplateQuery
{
    public class QueryCondition
    {
        
        public PropertyModel Property { get; set; }
        public OperathorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }


    internal static class QueryConditionExtensions
    {
        private static string MakeBinaryOperation(this QueryCondition condition, string operand, int token)
        {
            return string.Format("{0}{1}@{2}", condition.Property.Name, operand, token);
        }


        public static string BuildCondition(this QueryCondition condition, int token)
        {
            var operand = string.Empty;
            var value = string.Empty;

            switch (condition.Term.Operathor)
            {
                case Operathor.Equals:
                    operand =   " == ";
                    break;
                case Operathor.NotEquals:
                    operand = " != ";
                    break;
                case Operathor.GreaterThan:
                    operand = " > ";
                    break;
                case Operathor.GreaterThanEqualTo:
                    operand = " >= ";
                    break;
                case Operathor.LessThan:
                    operand = " < ";
                    break;
                case Operathor.LessThanEqualTo:
                    operand = " <= ";
                    break;
                case Operathor.Contains:
                    value = string.Format("{0}.Contains(@{1})", condition.Property.Name, token);
                    break;
                case Operathor.NotContains:
                    value =  string.Format("!{0}.Contains(@{1})", condition.Property.Name, token);
                    break;
                default :
                    operand = " == ";
                    break;
            }

            return string.IsNullOrEmpty(value) ? condition.MakeBinaryOperation(operand, token) : value;
        }

    }
}