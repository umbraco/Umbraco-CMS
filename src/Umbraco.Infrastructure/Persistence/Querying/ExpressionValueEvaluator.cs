using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Evaluates the value of an expression that does not depend on any lambda parameter, such as a captured variable
///     or a member of a captured object, without emitting and JIT-compiling code.
/// </summary>
internal static class ExpressionValueEvaluator
{
    /// <summary>
    ///     Gets the value of the specified expression.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The value of the expression, boxed as an object.</returns>
    /// <remarks>
    ///     Constants, and chains of field and property accesses over them, are read directly. Any other expression is
    ///     evaluated by the expression interpreter. Expression trees are rebuilt each time the code declaring them runs,
    ///     so compiling them to IL would generate a new dynamic method for every query.
    /// </remarks>
    public static object? GetValue(Expression expression)
        => TryGetValueDirectly(expression, out var value)
            ? value
            : Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object)))
                .Compile(preferInterpretation: true)();

    private static bool TryGetValueDirectly(Expression expression, out object? value)
    {
        switch (expression)
        {
            case ConstantExpression constant:
                value = constant.Value;
                return true;

            case MemberExpression member:
                return TryGetMemberValue(member, out value);

            case UnaryExpression { NodeType: ExpressionType.Convert } convert when IsValuePreservingConversion(convert):
                return TryGetValueDirectly(convert.Operand, out value);

            default:
                value = null;
                return false;
        }
    }

    private static bool TryGetMemberValue(MemberExpression member, out object? value)
    {
        value = null;

        object? target = null;
        if (member.Expression is not null)
        {
            // A null target is left to the interpreter so it throws the same exception compiled code would.
            if (TryGetValueDirectly(member.Expression, out target) is false || target is null)
            {
                return false;
            }
        }

        switch (member.Member)
        {
            case FieldInfo field:
                value = field.GetValue(target);
                return true;

            case PropertyInfo property when property.GetIndexParameters().Length == 0:
                value = property.GetValue(target, BindingFlags.DoNotWrapExceptions, null, null, null);
                return true;

            default:
                return false;
        }
    }

    // Boxing a value as itself, as object, or as its nullable form gives the same boxed value, so these conversions
    // can be skipped. Any other conversion may change the value (for example an enum to its underlying type).
    private static bool IsValuePreservingConversion(UnaryExpression convert)
        => convert.Method is null
           && (convert.Type == typeof(object)
               || convert.Type == convert.Operand.Type
               || Nullable.GetUnderlyingType(convert.Type) == convert.Operand.Type);
}
