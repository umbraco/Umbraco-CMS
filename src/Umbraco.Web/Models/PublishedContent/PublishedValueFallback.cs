using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedValueFallback"/>.
    /// </summary>
    public class PublishedValueFallback : IPublishedValueFallback
    {
        // this is our default implementation
        // kinda reproducing what was available in v7

        /// <inheritdoc />
        public object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, bool recurse)
        {
            // no fallback here
            if (!recurse) return defaultValue;

            // is that ok?
            return GetValue<object>(content, alias, culture, segment, defaultValue, recurse);
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse)
        {
            // no fallback here
            if (!recurse) return defaultValue;

            // otherwise, implement recursion as it was implemented in PublishedContentBase

            // fixme caching?
            //
            // all caches were using PublishedContentBase.GetProperty(alias, recurse) to get the property,
            // then,
            // NuCache.PublishedContent was storing the property in GetAppropriateCache() with key "NuCache.Property.Recurse[" + DraftOrPub(previewing) + contentUid + ":" + typeAlias + "]";
            // XmlPublishedContent was storing the property in _cacheProvider with key $"XmlPublishedCache.PublishedContentCache:RecursiveProperty-{Id}-{alias.ToLowerInvariant()}";
            // DictionaryPublishedContent was storing the property in _cacheProvider with key $"XmlPublishedCache.PublishedMediaCache:RecursiveProperty-{Id}-{alias.ToLowerInvariant()}";
            //
            // at the moment, caching has been entirely removed, until we better understand caching + fallback

            IPublishedProperty property = null; // if we are here, content's property has no value
            IPublishedProperty noValueProperty = null;
            do
            {
                content = content.Parent;
                property = content?.GetProperty(alias);
                if (property != null) noValueProperty = property;
            } while (content != null && (property == null || property.HasValue(culture, segment) == false));

            // if we found a content with the property having a value, return that property value
            if (property != null && property.HasValue(culture, segment))
                return property.Value<T>(culture, segment);

            // if we found a property, even though with no value, return that property value
            // because the converter may want to handle the missing value. ie if defaultValue is default,
            // either specified or by default, the converter may want to substitute something else.
            if (noValueProperty != null)
                return noValueProperty.Value<T>(culture, segment, defaultValue: defaultValue);

            // else return default
            return defaultValue;
        }
    }
}
