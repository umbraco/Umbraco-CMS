using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class FriendlyPublishedElementExtensions
{
    private static IPublishedValueFallback? _publishedValueFallback;
    private static IVariationContextAccessor? _variationContextAccessor;
    private static IUserService? _userService;

    private static IPublishedValueFallback PublishedValueFallback
    {
        get
        {
            _publishedValueFallback ??= StaticServiceProvider.Instance.GetRequiredService<IPublishedValueFallback>();
            return _publishedValueFallback;
        }
    }

    private static IVariationContextAccessor VariationContextAccessor
    {
        get
        {
            _variationContextAccessor ??= StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>();
            return _variationContextAccessor;
        }
    }

    private static IUserService UserService
    {
        get
        {
            _userService ??= StaticServiceProvider.Instance.GetRequiredService<IUserService>();
            return _userService;
        }
    }

    internal static void Reset()
    {
        _publishedValueFallback = null;
        _variationContextAccessor = null;
        _userService = null;
    }

    /// <summary>
    ///     Gets the name of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static string Name(
        this IPublishedElement content,
        string? culture = null)
        => content.Name(VariationContextAccessor, culture);

    /// <summary>
    ///     Gets the culture date of the content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">
    ///     The specific culture to get the name for. If null is used the current culture is used (Default is
    ///     null).
    /// </param>
    public static DateTime CultureDate(
        this IPublishedElement content,
        string? culture = null)
        => content.CultureDate(VariationContextAccessor, culture);

    /// <summary>
    ///     Gets the value of a content's property identified by its alias.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, returns
    ///         <paramref name="defaultValue" />.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    public static object? Value(
        this IPublishedElement content,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        object? defaultValue = default)
        => content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);

    /// <summary>
    ///     Gets the value of a content's property identified by its alias, converted to a specified type.
    /// </summary>
    /// <typeparam name="T">The target property type.</typeparam>
    /// <param name="content">The content.</param>
    /// <param name="alias">The property alias.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="segment">The variation segment.</param>
    /// <param name="fallback">Optional fallback strategy.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
    /// <remarks>
    ///     <para>
    ///         The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering
    ///         content.
    ///     </para>
    ///     <para>
    ///         If no property with the specified alias exists, or if the property has no value, or if it could not be
    ///         converted, returns <c>default(T)</c>.
    ///     </para>
    ///     <para>
    ///         If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the
    ///         converter.
    ///     </para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    public static T? Value<T>(
        this IPublishedElement content,
        string alias,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        T? defaultValue = default)
        => content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);

    /// <summary>
    ///     Gets a value indicating whether the content is visible.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>A value indicating whether the content is visible.</returns>
    /// <remarks>
    ///     A content is not visible if it has an umbracoNaviHide property with a value of "1". Otherwise,
    ///     the content is visible.
    /// </remarks>
    public static bool IsVisible(this IPublishedElement content) => content.IsVisible(PublishedValueFallback);

    /// <summary>
    ///     Gets the name of the content item creator.
    /// </summary>
    /// <param name="content">The content item.</param>
    public static string? CreatorName(this IPublishedElement content) =>
        content.CreatorName(UserService);

    /// <summary>
    ///     Gets the name of the content item writer.
    /// </summary>
    /// <param name="content">The content item.</param>
    public static string? WriterName(this IPublishedElement content) =>
        content.WriterName(UserService);

    /// <summary>
    ///     Gets the value of a property.
    /// </summary>
    public static TValue? ValueFor<TModel, TValue>(
        this TModel model,
        Expression<Func<TModel, TValue>> property,
        string? culture = null,
        string? segment = null,
        Fallback fallback = default,
        TValue? defaultValue = default)
        where TModel : IPublishedElement =>
        model.ValueFor(PublishedValueFallback, property, culture, segment, fallback);
}
