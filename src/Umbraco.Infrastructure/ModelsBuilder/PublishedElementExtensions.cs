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
    ///     Gets the value of a property.
    /// </summary>
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
        return model.Value(CheckVariationContext(publishedValueFallback,culture,segment), alias, culture, segment, fallback, defaultValue);
    }

    /// <summary>
    /// Method to check if VariationContext culture differs from culture parameter, if so it will update the VariationContext for the PublishedValueFallback.
    /// </summary>
    /// <param name="publishedValueFallback">The requested PublishedValueFallback.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="segment">The requested segment.</param>
    /// <returns></returns>
    private static IPublishedValueFallback CheckVariationContext(IPublishedValueFallback publishedValueFallback, string? culture, string? segment)
    {
        IVariationContextAccessor? variationContextAccessor = publishedValueFallback.VariationContextAccessor;

        //If there is a difference in requested culture and the culture that is set in the VariationContext, it will pick wrong localized content.
        //This happens for example using links to localized content in a RichText Editor.
        if (!string.IsNullOrEmpty(culture) && variationContextAccessor?.VariationContext?.Culture != culture)
        {
            variationContextAccessor!.VariationContext = new VariationContext(culture, segment);
        }

        return publishedValueFallback!;
    }

    // fixme that one should be public so ppl can use it
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
