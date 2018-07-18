using Umbraco.Core.Composing;

namespace Umbraco.Core.Models.PublishedContent
{
    public enum PublishedValueFallbackPriority
    {
        RecursiveTree,
        FallbackLanguage
    }

    /// <summary>
    /// Provides a fallback strategy for getting <see cref="IPublishedElement"/> values.
    /// </summary>
    // fixme - IPublishedValueFallback is still WorkInProgress
    // todo - properly document methods, etc
    // todo - understand caching vs fallback (recurse etc)
    public interface IPublishedValueFallback
    {
        // note that at property level, property.GetValue() does NOT implement fallback, and one has
        // to get property.Value() or property.Value<T>() to trigger fallback

        // this method is called whenever property.Value(culture, segment, defaultValue) is called, and
        // property.HasValue(culture, segment) is false. it can only fallback at property level (no recurse).

        object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue);

        // this method is called whenever property.Value<T>(culture, segment, defaultValue) is called, and
        // property.HasValue(culture, segment) is false. it can only fallback at property level (no recurse).

        T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue);

        // these methods to be called whenever getting the property value for the specified alias, culture and segment,
        // either returned no property at all, or a property that does not HasValue for the specified culture and segment.

        object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue);

        T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue);

        object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority);

        T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority);
    }
}
