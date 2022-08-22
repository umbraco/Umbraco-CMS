namespace Umbraco.Cms.Core.Models.TemplateQuery;

public static class OperatorFactory
{
    public static Operator FromString(string stringOperator)
    {
        if (stringOperator == null)
        {
            throw new ArgumentNullException(nameof(stringOperator));
        }

        switch (stringOperator)
        {
            case "=":
            case "==":
                return Operator.Equals;
            case "!=":
            case "<>":
                return Operator.NotEquals;
            case "<":
                return Operator.LessThan;
            case "<=":
                return Operator.LessThanEqualTo;
            case ">":
                return Operator.GreaterThan;
            case ">=":
                return Operator.GreaterThanEqualTo;
            default:
                throw new ArgumentException(
                    $"A operator cannot be created from the specified string '{stringOperator}'",
                    nameof(stringOperator));
        }
    }
}
