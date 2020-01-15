using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Web.Models.TemplateQuery
{
    internal static class QueryConditionExtensions
    {
        private static Lazy<MethodInfo> StringContainsMethodInfo =>
            new Lazy<MethodInfo>(() => typeof(string).GetMethod("Contains", new[] {typeof(string)}));

        public static Expression<Func<T, bool>> BuildCondition<T>(this QueryCondition condition, string parameterAlias)
        {
            object constraintValue;
            switch (condition.Property.Type.ToLowerInvariant())
            {
                case "string":
                    constraintValue = condition.ConstraintValue;
                    break;
                case "datetime":
                    constraintValue = DateTime.Parse(condition.ConstraintValue);
                    break;
                case "boolean":
                    constraintValue = Boolean.Parse(condition.ConstraintValue);
                    break;
                default:
                    constraintValue = Convert.ChangeType(condition.ConstraintValue, typeof(int));
                    break;
            }

            var parameterExpression = Expression.Parameter(typeof(T), parameterAlias);
            var propertyExpression = Expression.Property(parameterExpression, condition.Property.Alias);

            var valueExpression = Expression.Constant(constraintValue);
            Expression bodyExpression;
            switch (condition.Term.Operator)
            {
                case Operator.NotEquals:
                    bodyExpression = Expression.NotEqual(propertyExpression, valueExpression);
                    break;
                case Operator.GreaterThan:
                    bodyExpression = Expression.GreaterThan(propertyExpression, valueExpression);
                    break;
                case Operator.GreaterThanEqualTo:
                    bodyExpression = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
                    break;
                case Operator.LessThan:
                    bodyExpression = Expression.LessThan(propertyExpression, valueExpression);
                    break;
                case Operator.LessThanEqualTo:
                    bodyExpression = Expression.LessThanOrEqual(propertyExpression, valueExpression);
                    break;
                case Operator.Contains:
                    bodyExpression = Expression.Call(propertyExpression, StringContainsMethodInfo.Value,
                        valueExpression);
                    break;
                case Operator.NotContains:
                    var tempExpression = Expression.Call(propertyExpression, StringContainsMethodInfo.Value,
                        valueExpression);
                    bodyExpression = Expression.Equal(tempExpression, Expression.Constant(false));
                    break;
                default:
                case Operator.Equals:
                    bodyExpression = Expression.Equal(propertyExpression, valueExpression);
                    break;
            }

            var predicate =
                Expression.Lambda<Func<T, bool>>(bodyExpression.Reduce(), parameterExpression);

            return predicate;
        }
    }
}