using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     This is called from within the generated model classes
/// </summary>
/// <remarks>
///     DO NOT REMOVE - although there are not code references this is used directly by the generated models.
/// </remarks>
public static class PublishedModelUtility
{
    // looks safer but probably useless... ppl should not call these methods directly
    // and if they do... they have to take care about not doing stupid things

    // public static PublishedPropertyType GetModelPropertyType2<T>(Expression<Func<T, object>> selector)
    //    where T : PublishedContentModel
    // {
    //    var type = typeof (T);
    //    var s1 = type.GetField("ModelTypeAlias", BindingFlags.Public | BindingFlags.Static);
    //    var alias = (s1.IsLiteral && s1.IsInitOnly && s1.FieldType == typeof(string)) ? (string)s1.GetValue(null) : null;
    //    var s2 = type.GetField("ModelItemType", BindingFlags.Public | BindingFlags.Static);
    //    var itemType = (s2.IsLiteral && s2.IsInitOnly && s2.FieldType == typeof(PublishedItemType)) ? (PublishedItemType)s2.GetValue(null) : 0;

    /// <summary>
    /// Retrieves the published content type from the specified content type cache using the given item type and alias.
    /// </summary>
    /// <remarks>
    /// var contentType = PublishedContentType.Get(itemType, alias);
    /// // etc...
    /// }
    /// </remarks>
    /// <param name="contentTypeCache">The cache from which to retrieve the published content type.</param>
    /// <param name="itemType">The type of published item (e.g., Content, Media, Member, or Element).</param>
    /// <param name="alias">The alias identifying the content type.</param>
    /// <returns>The <see cref="IPublishedContentType"/> corresponding to the specified item type and alias.</returns>
    public static IPublishedContentType GetModelContentType(
        IPublishedContentTypeCache contentTypeCache,
        PublishedItemType itemType,
        string alias)
    {
        switch (itemType)
        {
            case PublishedItemType.Content:
                return contentTypeCache.Get(PublishedItemType.Content, alias);
            case PublishedItemType.Element:
                return contentTypeCache.Get(PublishedItemType.Element, alias);
            case PublishedItemType.Media:
                return contentTypeCache.Get(PublishedItemType.Media, alias);
            case PublishedItemType.Member:
                return contentTypeCache.Get(PublishedItemType.Member, alias);
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType));
        }
    }

    /// <summary>
    /// Gets the published property type for the specified property selector on the given content type.
    /// </summary>
    /// <typeparam name="TModel">The model type, typically derived from <c>PublishedContentModel</c> or <c>PublishedElementModel</c>.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="contentType">The published content type to retrieve the property type from.</param>
    /// <param name="selector">An expression selecting the property on the model.</param>
    /// <returns>The published property type if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="selector"/> is not a property expression.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the property alias cannot be determined from the selector.</exception>
    public static IPublishedPropertyType? GetModelPropertyType<TModel, TValue>(
        IPublishedContentType contentType,
        Expression<Func<TModel, TValue>> selector)

    // where TModel : PublishedContentModel // TODO: PublishedContentModel _or_ PublishedElementModel
    {
        // TODO therefore, missing a check on TModel here
        if (selector.Body is not MemberExpression expr)
        {
            throw new ArgumentException("Not a property expression.", nameof(selector));
        }

        // there _is_ a risk that contentType and T do not match
        // see note above : accepted risk...
        ImplementPropertyTypeAttribute? attr = expr.Member
            .GetCustomAttributes(typeof(ImplementPropertyTypeAttribute), false)
            .OfType<ImplementPropertyTypeAttribute>()
            .SingleOrDefault();

        if (string.IsNullOrWhiteSpace(attr?.Alias))
        {
            throw new InvalidOperationException(
                $"Could not figure out property alias for property \"{expr.Member.Name}\".");
        }

        return contentType.GetPropertyType(attr.Alias);
    }
}
