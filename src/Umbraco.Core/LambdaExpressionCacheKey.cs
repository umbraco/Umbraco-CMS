using System.Linq.Expressions;

namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a simple <see cref="LambdaExpression" /> in a form which is suitable for using as a dictionary key
///     by exposing the return type, argument types and expression string form in a single concatenated string.
/// </summary>
public struct LambdaExpressionCacheKey
{
    /// <summary>
    ///     The argument type names of the <see cref="LambdaExpression" />
    /// </summary>
    public readonly HashSet<string?> ArgTypes;

    public LambdaExpressionCacheKey(string returnType, string expression, params string[] argTypes)
    {
        ReturnType = returnType;
        ExpressionAsString = expression;
        ArgTypes = new HashSet<string?>(argTypes);
        _toString = null;
    }

    public LambdaExpressionCacheKey(LambdaExpression obj)
    {
        ReturnType = obj.ReturnType.FullName;
        ExpressionAsString = obj.ToString();
        ArgTypes = new HashSet<string?>(obj.Parameters.Select(x => x.Type.FullName));
        _toString = null;
    }

    /// <summary>
    ///     The return type of the <see cref="LambdaExpression" />
    /// </summary>
    public readonly string? ReturnType;

    /// <summary>
    ///     The original string representation of the <see cref="LambdaExpression" />
    /// </summary>
    public readonly string ExpressionAsString;

    private string? _toString;

    /// <summary>
    ///     Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => _toString ??= string.Concat(string.Join("|", ArgTypes), ",", ReturnType, ",", ExpressionAsString);

    /// <summary>
    ///     Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        var casted = (LambdaExpressionCacheKey)obj;
        return casted.ToString() == ToString();
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode() => ToString().GetHashCode();
}
