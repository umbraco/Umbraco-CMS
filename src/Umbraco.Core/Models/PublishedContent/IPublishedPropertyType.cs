using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published property type.
/// </summary>
/// <remarks>
///     Instances implementing the <see cref="PublishedPropertyType" /> interface should be
///     immutable, ie if the property type changes, then a new instance needs to be created.
/// </remarks>
public interface IPublishedPropertyType
{
    /// <summary>
    ///     Gets the published content type containing the property type.
    /// </summary>
    IPublishedContentType? ContentType { get; }

    /// <summary>
    ///     Gets the data type.
    /// </summary>
    PublishedDataType DataType { get; }

    /// <summary>
    ///     Gets property type alias.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the property editor alias.
    /// </summary>
    string EditorAlias { get; }

    /// <summary>
    ///     Gets a value indicating whether the property is a user content property.
    /// </summary>
    /// <remarks>
    ///     A non-user content property is a property that has been added to a
    ///     published content type by Umbraco but does not corresponds to a user-defined
    ///     published property.
    /// </remarks>
    bool IsUserProperty { get; }

    /// <summary>
    ///     Gets the content variations of the property type.
    /// </summary>
    ContentVariation Variations { get; }

    /// <summary>
    ///     Gets the property cache level.
    /// </summary>
    PropertyCacheLevel CacheLevel { get; }

    /// <summary>
    ///     Gets the property model CLR type.
    /// </summary>
    /// <remarks>
    ///     <para>The model CLR type may be a <see cref="ModelType" /> type, or may contain <see cref="ModelType" /> types.</para>
    ///     <para>For the actual CLR type, see <see cref="ClrType" />.</para>
    /// </remarks>
    Type ModelClrType { get; }

    /// <summary>
    ///     Gets the property CLR type.
    /// </summary>
    /// <remarks>
    ///     <para>Returns the actual CLR type which does not contain <see cref="ModelType" /> types.</para>
    ///     <para>
    ///         Mapping from <see cref="ModelClrType" /> may throw if some <see cref="ModelType" /> instances
    ///         could not be mapped to actual CLR types.
    ///     </para>
    /// </remarks>
    Type? ClrType { get; }

    /// <summary>
    ///     Determines whether a value is an actual value, or not a value.
    /// </summary>
    /// <remarks>Used by property.HasValue and, for instance, in fallback scenarios.</remarks>
    bool? IsValue(object? value, PropertyValueLevel level);

    /// <summary>
    ///     Converts the source value into the intermediate value.
    /// </summary>
    /// <param name="owner">The published element owning the property.</param>
    /// <param name="source">The source value.</param>
    /// <param name="preview">A value indicating whether content should be considered draft.</param>
    /// <returns>The intermediate value.</returns>
    object? ConvertSourceToInter(IPublishedElement owner, object? source, bool preview);

    /// <summary>
    ///     Converts the intermediate value into the object value.
    /// </summary>
    /// <param name="owner">The published element owning the property.</param>
    /// <param name="referenceCacheLevel">The reference cache level.</param>
    /// <param name="inter">The intermediate value.</param>
    /// <param name="preview">A value indicating whether content should be considered draft.</param>
    /// <returns>The object value.</returns>
    object? ConvertInterToObject(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview);

    /// <summary>
    ///     Converts the intermediate value into the XPath value.
    /// </summary>
    /// <param name="owner">The published element owning the property.</param>
    /// <param name="referenceCacheLevel">The reference cache level.</param>
    /// <param name="inter">The intermediate value.</param>
    /// <param name="preview">A value indicating whether content should be considered draft.</param>
    /// <returns>The XPath value.</returns>
    /// <remarks>
    ///     <para>The XPath value can be either a string or an XPathNavigator.</para>
    /// </remarks>
    object? ConvertInterToXPath(IPublishedElement owner, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview);
}
