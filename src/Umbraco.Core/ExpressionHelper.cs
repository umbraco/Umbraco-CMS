using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Core;

/// <summary>
///     A set of helper methods for dealing with expressions
/// </summary>
/// <remarks></remarks>
public static class ExpressionHelper
{
    private static readonly ConcurrentDictionary<LambdaExpressionCacheKey, PropertyInfo> PropertyInfoCache = new();

    /// <summary>
    ///     Gets a <see cref="PropertyInfo" /> object from an expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertyLambda">The property lambda.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
        this TSource source,
        Expression<Func<TSource, TProperty>> propertyLambda) => GetPropertyInfo(propertyLambda);

    /// <summary>
    ///     Gets a <see cref="PropertyInfo" /> object from an expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyLambda">The property lambda.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static PropertyInfo
        GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda) =>
        PropertyInfoCache.GetOrAdd(
            new LambdaExpressionCacheKey(propertyLambda),
            x =>
            {
                Type type = typeof(TSource);

                var member = propertyLambda.Body as MemberExpression;
                if (member == null)
                {
                    if (propertyLambda.Body.GetType().Name == "UnaryExpression")
                    {
                        // The expression might be for some boxing, e.g. representing a value type like HiveId as an object
                        // in which case the expression will be Convert(x.MyProperty)
                        if (propertyLambda.Body is UnaryExpression unary)
                        {
                            if (unary.Operand is not MemberExpression boxedMember)
                            {
                                throw new ArgumentException(
                                    "The type of property could not be inferred, try specifying the type parameters explicitly. This can happen if you have tried to access PropertyInfo where the property's return type is a value type, but the expression is trying to convert it to an object");
                            }

                            member = boxedMember;
                        }
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda));
                    }
                }

                var propInfo = member!.Member as PropertyInfo;
                if (propInfo == null)
                {
                    throw new ArgumentException(string.Format(
                        "Expression '{0}' refers to a field, not a property.",
                        propertyLambda));
                }

                if (type != propInfo.ReflectedType &&
                    !type.IsSubclassOf(propInfo.ReflectedType!))
                {
                    throw new ArgumentException(string.Format(
                        "Expression '{0}' refers to a property that is not from type {1}.",
                        propertyLambda,
                        type));
                }

                return propInfo;
            });

    public static (MemberInfo, string?) FindProperty(LambdaExpression lambda)
    {
        void Throw()
        {
            throw new ArgumentException(
                $"Expression '{lambda}' must resolve to top-level member and not any child object's properties. Use a custom resolver on the child type or the AfterMap option instead.",
                nameof(lambda));
        }

        Expression expr = lambda;
        var loop = true;
        string? alias = null;
        while (loop)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Convert:
                    expr = ((UnaryExpression)expr).Operand;
                    break;
                case ExpressionType.Lambda:
                    expr = ((LambdaExpression)expr).Body;
                    break;
                case ExpressionType.Call:
                    var callExpr = (MethodCallExpression)expr;
                    MethodInfo method = callExpr.Method;
                    if (method.DeclaringType != typeof(SqlExtensionsStatics) || method.Name != "Alias" ||
                        !(callExpr.Arguments[1] is ConstantExpression aliasExpr))
                    {
                        Throw();
                    }

                    expr = callExpr.Arguments[0];
                    alias = aliasExpr.Value?.ToString();
                    break;
                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expr;
                    if (memberExpr.Expression?.NodeType != ExpressionType.Parameter &&
                        memberExpr.Expression?.NodeType != ExpressionType.Convert)
                    {
                        Throw();
                    }

                    return (memberExpr.Member, alias);
                default:
                    loop = false;
                    break;
            }
        }

        throw new Exception("Configuration for members is only supported for top-level individual members on a type.");
    }

    public static IDictionary<string, object?>? GetMethodParams<T1, T2>(Expression<Func<T1, T2>> fromExpression)
    {
        if (fromExpression == null)
        {
            return null;
        }

        if (fromExpression.Body is not MethodCallExpression body)
        {
            return new Dictionary<string, object?>();
        }

        var rVal = new Dictionary<string, object?>();
        var parameters = body.Method.GetParameters().Select(x => x.Name).Where(x => x is not null).ToArray();
        var i = 0;
        foreach (Expression argument in body.Arguments)
        {
            LambdaExpression lambda = Expression.Lambda(argument, fromExpression.Parameters);
            Delegate d = lambda.Compile();
            var value = d.DynamicInvoke(new object[1]);
            rVal.Add(parameters[i]!, value);
            i++;
        }

        return rVal;
    }

    /// <summary>
    ///     Gets a <see cref="MethodInfo" /> from an <see cref="Expression{Action{T}}" /> provided it refers to a method call.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fromExpression">From expression.</param>
    /// <returns>
    ///     The <see cref="MethodInfo" /> or null if <paramref name="fromExpression" /> is null or cannot be converted to
    ///     <see cref="MethodCallExpression" />.
    /// </returns>
    /// <remarks></remarks>
    public static MethodInfo? GetMethodInfo<T>(Expression<Action<T>> fromExpression)
    {
        if (fromExpression == null)
        {
            return null;
        }

        var body = fromExpression.Body as MethodCallExpression;
        return body?.Method;
    }

    /// <summary>
    ///     Gets the method info.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the method.</typeparam>
    /// <param name="fromExpression">From expression.</param>
    /// <returns></returns>
    public static MethodInfo? GetMethodInfo<TReturn>(Expression<Func<TReturn>> fromExpression)
    {
        if (fromExpression == null)
        {
            return null;
        }

        var body = fromExpression.Body as MethodCallExpression;
        return body?.Method;
    }

    /// <summary>
    ///     Gets the method info.
    /// </summary>
    /// <typeparam name="T1">The type of the 1.</typeparam>
    /// <typeparam name="T2">The type of the 2.</typeparam>
    /// <param name="fromExpression">From expression.</param>
    /// <returns></returns>
    public static MethodInfo? GetMethodInfo<T1, T2>(Expression<Func<T1, T2>> fromExpression)
    {
        if (fromExpression == null)
        {
            return null;
        }

        MethodCallExpression? me;
        switch (fromExpression.Body.NodeType)
        {
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                var ue = fromExpression.Body as UnaryExpression;
                me = ue?.Operand as MethodCallExpression;
                break;
            default:
                me = fromExpression.Body as MethodCallExpression;
                break;
        }

        return me?.Method;
    }

    /// <summary>
    ///     Gets a <see cref="MethodInfo" /> from an <see cref="Expression" /> provided it refers to a method call.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    ///     The <see cref="MethodInfo" /> or null if <paramref name="expression" /> cannot be converted to
    ///     <see cref="MethodCallExpression" />.
    /// </returns>
    /// <remarks></remarks>
    public static MethodInfo? GetMethod(Expression expression)
    {
        if (expression == null)
        {
            return null;
        }

        return IsMethod(expression) ? ((MethodCallExpression)expression).Method : null;
    }

    /// <summary>
    ///     Gets a <see cref="MemberInfo" /> from an <see cref="Expression{Func{T, TReturn}}" /> provided it refers to member
    ///     access.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    /// <param name="fromExpression">From expression.</param>
    /// <returns>
    ///     The <see cref="MemberInfo" /> or null if <paramref name="fromExpression" /> cannot be converted to
    ///     <see cref="MemberExpression" />.
    /// </returns>
    /// <remarks></remarks>
    public static MemberInfo? GetMemberInfo<T, TReturn>(Expression<Func<T, TReturn>> fromExpression)
    {
        if (fromExpression == null)
        {
            return null;
        }

        MemberExpression? me;
        switch (fromExpression.Body.NodeType)
        {
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                var ue = fromExpression.Body as UnaryExpression;
                me = ue?.Operand as MemberExpression;
                break;
            default:
                me = fromExpression.Body as MemberExpression;
                break;
        }

        return me?.Member;
    }

    /// <summary>
    ///     Determines whether the MethodInfo is the same based on signature, not based on the equality operator or HashCode.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    ///     <c>true</c> if [is method signature equal to] [the specified left]; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This is useful for comparing Expression methods that may contain different generic types
    /// </remarks>
    public static bool IsMethodSignatureEqualTo(this MethodInfo left, MethodInfo right)
    {
        if (left.Equals(right))
        {
            return true;
        }

        if (left.DeclaringType != right.DeclaringType)
        {
            return false;
        }

        if (left.Name != right.Name)
        {
            return false;
        }

        ParameterInfo[] leftParams = left.GetParameters();
        ParameterInfo[] rightParams = right.GetParameters();
        if (leftParams.Length != rightParams.Length)
        {
            return false;
        }

        for (var i = 0; i < leftParams.Length; i++)
        {
            // if they are delegate parameters, then assume they match as they could be anything
            if (typeof(Delegate).IsAssignableFrom(leftParams[i].ParameterType) &&
                typeof(Delegate).IsAssignableFrom(rightParams[i].ParameterType))
            {
                continue;
            }

            // if they are not delegates, then compare the types
            if (leftParams[i].ParameterType != rightParams[i].ParameterType)
            {
                return false;
            }
        }

        if (left.ReturnType != right.ReturnType)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="MemberInfo" /> from an <see cref="Expression" /> provided it refers to member access.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static MemberInfo? GetMember(Expression expression)
    {
        if (expression == null)
        {
            return null;
        }

        return IsMember(expression) ? ((MemberExpression)expression).Member : null;
    }

    /// <summary>
    ///     Gets a <see cref="MethodInfo" /> from a <see cref="Delegate" />
    /// </summary>
    /// <param name="fromMethodGroup">From method group.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static MethodInfo GetStaticMethodInfo(Delegate fromMethodGroup)
    {
        if (fromMethodGroup == null)
        {
            throw new ArgumentNullException("fromMethodGroup");
        }

        return fromMethodGroup.Method;
    }

    ///// <summary>
    ///// Formats an unhandled item for representing the expression as a string.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="unhandledItem">The unhandled item.</param>
    ///// <returns></returns>
    ///// <remarks></remarks>
    // public static string FormatUnhandledItem<T>(T unhandledItem) where T : class
    // {
    //    if (unhandledItem == null) throw new ArgumentNullException("unhandledItem");

    // var itemAsExpression = unhandledItem as Expression;
    //    return itemAsExpression != null
    //               ? FormattingExpressionTreeVisitor.Format(itemAsExpression)
    //               : unhandledItem.ToString();
    // }

    /// <summary>
    ///     Determines whether the specified expression is a method.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><c>true</c> if the specified expression is method; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool IsMethod(Expression expression) => expression is MethodCallExpression;

    /// <summary>
    ///     Determines whether the specified expression is a member.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><c>true</c> if the specified expression is member; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool IsMember(Expression expression) => expression is MemberExpression;

    /// <summary>
    ///     Determines whether the specified expression is a constant.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><c>true</c> if the specified expression is constant; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool IsConstant(Expression expression) => expression is ConstantExpression;

    /// <summary>
    ///     Gets the first value from the supplied arguments of an expression, for those arguments that can be cast to
    ///     <see cref="ConstantExpression" />.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static object? GetFirstValueFromArguments(IEnumerable<Expression> arguments)
    {
        if (arguments == null)
        {
            return false;
        }

        return
            arguments.Where(x => x is ConstantExpression).Cast
                <ConstantExpression>().Select(x => x.Value).DefaultIfEmpty(null).FirstOrDefault();
    }
}
