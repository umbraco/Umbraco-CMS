using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

// same namespace as original Umbraco.Web PublishedElementExtensions
// ReSharper disable once CheckNamespace
namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to models.
/// </summary>
public static class PublishedElementExtensions
{
    /// <summary>
    ///     Gets the value of a specified property from the model.
    /// </summary>
    /// <param name="model">The published element model from which to retrieve the property value.</param>
    /// <param name="publishedValueFallback">The fallback strategy to use when resolving the property value.</param>
    /// <param name="property">An expression that identifies the property to retrieve.</param>
    /// <param name="culture">The culture to use when retrieving the value, or <c>null</c> for the default culture.</param>
    /// <param name="segment">The segment to use when retrieving the value, or <c>null</c> if not applicable.</param>
    /// <param name="fallback">The fallback behavior to apply if the value is not found.</param>
    /// <param name="defaultValue">The value to return if the property value is not found.</param>
    /// <returns>The value of the specified property, or <paramref name="defaultValue"/> if not found.</returns>
    public static TValue? ValueFor<TModel, TValue>(
        this TModel model,
        IPublishedValueFallback publishedValueFallback,
        Expression<Func<TModel, TValue>> property,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        TValue? defaultValue = default)
        where TModel : IPublishedElement
    {
        var alias = GetAlias(model, property);
        return model.Value(publishedValueFallback, alias, culture, segment, fallback, defaultValue);
    }

    // TODO: that one should be public so ppl can use it
    private static string GetAlias<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> property)
    {
        if (property.NodeType != ExpressionType.Lambda)
        {
            throw new ArgumentException("Not a proper lambda expression (lambda).", nameof(property));
        }

        var lambda = (LambdaExpression)property;
        Expression lambdaBody = lambda.Body;

        if (lambdaBody.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException("Not a proper lambda expression (body).", nameof(property));
        }

        var memberExpression = (MemberExpression)lambdaBody;
        if (memberExpression.Expression?.NodeType != ExpressionType.Parameter)
        {
            throw new ArgumentException("Not a proper lambda expression (member).", nameof(property));
        }

        MemberInfo member = memberExpression.Member;

        ImplementPropertyTypeAttribute? attribute = member.GetCustomAttribute<ImplementPropertyTypeAttribute>();
        if (attribute == null)
        {
            throw new InvalidOperationException("Property is not marked with ImplementPropertyType attribute.");
        }

        return attribute.Alias;
    }
}
