using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedValueFallback"/>.
    /// </summary>
    public class PublishedValueFallback : IPublishedValueFallback
    {
        // kinda reproducing what was available in v7

        /// <inheritdoc />
        public object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages)
        {
            // no fallback here
            return defaultValue;
        }

        /// <inheritdoc />
        public object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages)
        {
            // is that ok?
            return GetValue<object>(content, alias, culture, segment, defaultValue, fallbackMethods, visitedLanguages);
        }

        /// <inheritdoc />
        public T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages)
        {
            if (fallbackMethods == null)
            {
                return defaultValue;
            }

            foreach (var fallbackMethod in fallbackMethods)
            {
                if (TryGetValueWithFallbackMethod(content, alias, culture, segment, defaultValue, fallbackMethods, visitedLanguages, fallbackMethod, out T value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        protected virtual bool TryGetValueWithFallbackMethod<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages, int fallbackMethod, out T value)
        {
            value = defaultValue;
            switch (fallbackMethod)
            {
                case Core.Constants.Content.FallbackMethods.None:
                    return false;
                case Core.Constants.Content.FallbackMethods.RecursiveTree:
                    return TryGetValueWithRecursiveTree(content, alias, culture, segment, defaultValue, out value);
                default:
                    throw new NotSupportedException($"Fallback method with indentifying number {fallbackMethod} is not supported within {GetType().Name}.");
            }
        }

        protected static bool TryGetValueWithRecursiveTree<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, out T value)
        {
            // Implement recursion as it was implemented in PublishedContentBase

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
                if (property != null)
                {
                    noValueProperty = property;
                }
            }
            while (content != null && (property == null || property.HasValue(culture, segment) == false));

            // if we found a content with the property having a value, return that property value
            if (property != null && property.HasValue(culture, segment))
            {
                value = property.Value<T>(culture, segment);
                return true;
            }

            // if we found a property, even though with no value, return that property value
            // because the converter may want to handle the missing value. ie if defaultValue is default,
            // either specified or by default, the converter may want to substitute something else.
            if (noValueProperty != null)
            {
                value = noValueProperty.Value<T>(culture, segment, defaultValue: defaultValue);
                return true;
            }

            value = defaultValue;
            return false;
        }
    }
}
