// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <c>IPublishedProperty</c>.
/// </summary>
public static class PublishedPropertyExtension
{
    #region Value

    public static object? Value(this IPublishedProperty property, IPublishedValueFallback publishedValueFallback, string? culture = null, string? segment = null, Fallback fallback = default, object? defaultValue = default)
    {
        if (property.HasValue(culture, segment))
        {
            return property.GetValue(culture, segment);
        }

        return publishedValueFallback.TryGetValue(property, culture, segment, fallback, defaultValue, out var value)
            ? value
            : property.GetValue(culture, segment); // give converter a chance to return it's own vision of "no value"
    }

    #endregion

    #region Value<T>

    public static T? Value<T>(this IPublishedProperty property, IPublishedValueFallback publishedValueFallback, string? culture = null, string? segment = null, Fallback fallback = default, T? defaultValue = default)
    {
        if (property.HasValue(culture, segment))
        {
            // we have a value
            // try to cast or convert it
            var value = property.GetValue(culture, segment);
            if (value is T valueAsT)
            {
                return valueAsT;
            }

            Attempt<T> valueConverted = value.TryConvertTo<T>();
            if (valueConverted.Success)
            {
                return valueConverted.Result;
            }

            // cannot cast nor convert the value, nothing we can return but 'default'
            // note: we don't want to fallback in that case - would make little sense
            return default;
        }

        // we don't have a value, try fallback
        if (publishedValueFallback.TryGetValue(property, culture, segment, fallback, defaultValue, out T? fallbackValue))
        {
            return fallbackValue;
        }

        // we don't have a value - neither direct nor fallback
        // give a chance to the converter to return something (eg empty enumerable)
        var noValue = property.GetValue(culture, segment);
        if (noValue is T noValueAsT)
        {
            return noValueAsT;
        }

        Attempt<T> noValueConverted = noValue.TryConvertTo<T>();
        if (noValueConverted.Success)
        {
            return noValueConverted.Result;
        }

        // cannot cast noValue nor convert it, nothing we can return but 'default'
        return default;
    }

    #endregion
}
