using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for building query condition expressions.
/// </summary>
public static class QueryConditionExtensions
{
    private static Lazy<MethodInfo> StringContainsMethodInfo =>
        new(() => typeof(string).GetMethod("Contains", new[] { typeof(string) })!);

    /// <summary>
    ///     Builds a lambda expression predicate from a query condition.
    /// </summary>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="condition">The query condition to convert.</param>
    /// <param name="parameterAlias">The alias to use for the parameter in the expression.</param>
    /// <returns>A lambda expression that can be used to filter entities.</returns>
    public static Expression<Func<T, bool>> BuildCondition<T>(this QueryCondition condition, string parameterAlias)
    {
        object constraintValue;
        switch (condition.Property.Type?.ToLowerInvariant())
        {
            case "string":
                constraintValue = condition.ConstraintValue;
                break;
            case "datetime":
                constraintValue = DateTime.Parse(condition.ConstraintValue);
                break;
            case "boolean":
                constraintValue = bool.Parse(condition.ConstraintValue);
                break;
            default:
                constraintValue = Convert.ChangeType(condition.ConstraintValue, typeof(int));
                break;
        }

        ParameterExpression parameterExpression = Expression.Parameter(typeof(T), parameterAlias);
        PropertyInfo propertyInfo = GetPropertyInfo(typeof(T), condition.Property.Alias);
        MemberExpression propertyExpression = Expression.Property(parameterExpression, propertyInfo);

        ConstantExpression valueExpression = Expression.Constant(constraintValue);
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
                bodyExpression = Expression.Call(propertyExpression, StringContainsMethodInfo.Value, valueExpression);
                break;
            case Operator.NotContains:
                MethodCallExpression tempExpression = Expression.Call(
                    propertyExpression,
                    StringContainsMethodInfo.Value,
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

    /// <summary>
    ///     Gets the property with the specified name from the type or one of its inherited interfaces.
    /// </summary>
    /// <param name="type">The type to search for the property.</param>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The matching <see cref="PropertyInfo" />.</returns>
    /// <remarks>
    ///     <see cref="Type.GetProperty(string, BindingFlags)" /> on an interface does not return members declared on
    ///     base interfaces, so when the property is not found directly (e.g. <c>IPublishedContent</c> exposing
    ///     <c>Name</c>/<c>Id</c> via <c>IPublishedElement</c>) the inherited interfaces are searched explicitly. The
    ///     lookup is restricted to public instance properties to mirror <see cref="Expression.Property(Expression, string)" />.
    /// </remarks>
    private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

        PropertyInfo? propertyInfo = type.GetProperty(propertyName, bindingFlags);
        if (propertyInfo is not null)
        {
            return propertyInfo;
        }

        foreach (Type interfaceType in type.GetInterfaces())
        {
            propertyInfo = interfaceType.GetProperty(propertyName, bindingFlags);
            if (propertyInfo is not null)
            {
                return propertyInfo;
            }
        }

        throw new ArgumentException($"Instance property '{propertyName}' is not defined for type '{type}'", nameof(propertyName));
    }
}
