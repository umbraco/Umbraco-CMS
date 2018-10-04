using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedValueFallback"/>.
    /// </summary>
    public class PublishedValueFallback : IPublishedValueFallback
    {
        private readonly ILocalizationService _localizationService;
        private readonly IVariationContextAccessor _variationContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedValueFallback"/> class.
        /// </summary>
        public PublishedValueFallback(ServiceContext serviceContext, IVariationContextAccessor variationContextAccessor)
        {
            _localizationService = serviceContext.LocalizationService;
            _variationContextAccessor = variationContextAccessor;
        }

        /// <inheritdoc />
        public bool TryGetValue(IPublishedProperty property, string culture, string segment, Fallback fallback, object defaultValue, out object value)
        {
            return TryGetValue<object>(property, culture, segment, fallback, defaultValue, out value);
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(IPublishedProperty property, string culture, string segment, Fallback fallback, T defaultValue, out T value)
        {
            _variationContextAccessor.ContextualizeVariation(property.PropertyType.Variations, ref culture, ref segment);

            foreach (var f in fallback)
            {
                switch (f)
                {
                    case Fallback.None:
                        continue;
                    case Fallback.DefaultValue:
                        value = defaultValue;
                        return true;
                    case Fallback.Language:
                        if (TryGetValueWithLanguageFallback(property, culture, segment, defaultValue, out value))
                            return true;
                        break;
                    default:
                        throw NotSupportedFallbackMethod(f, "property");
                }
            }

            value = defaultValue;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(IPublishedElement content, string alias, string culture, string segment, Fallback fallback, object defaultValue, out object value)
        {
            return TryGetValue<object>(content, alias, culture, segment, fallback, defaultValue, out value);
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(IPublishedElement content, string alias, string culture, string segment, Fallback fallback, T defaultValue, out T value)
        {
            var propertyType = content.ContentType.GetPropertyType(alias);
            if (propertyType == null)
            {
                value = default;
                return false;
            }
            _variationContextAccessor.ContextualizeVariation(propertyType.Variations, ref culture, ref segment);

            foreach (var f in fallback)
            {
                switch (f)
                {
                    case Fallback.None:
                        continue;
                    case Fallback.DefaultValue:
                        value = defaultValue;
                        return true;
                    case Fallback.Language:
                        if (TryGetValueWithLanguageFallback(content, alias, culture, segment, defaultValue, out value))
                            return true;
                        break;
                    default:
                        throw NotSupportedFallbackMethod(f, "element");
                }
            }

            value = defaultValue;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(IPublishedContent content, string alias, string culture, string segment, Fallback fallback, object defaultValue, out object value)
        {
            // is that ok?
            return TryGetValue<object>(content, alias, culture, segment, fallback, defaultValue, out value);
        }

        /// <inheritdoc />
        public virtual bool TryGetValue<T>(IPublishedContent content, string alias, string culture, string segment, Fallback fallback, T defaultValue, out T value)
        {
            var propertyType = content.ContentType.GetPropertyType(alias);
            if (propertyType == null)
            {
                value = default;
                return false;
            }
            _variationContextAccessor.ContextualizeVariation(propertyType.Variations, ref culture, ref segment);

            // note: we don't support "recurse & language" which would walk up the tree,
            // looking at languages at each level - should someone need it... they'll have
            // to implement it.

            foreach (var f in fallback)
            {
                switch (f)
                {
                    case Fallback.None:
                        continue;
                    case Fallback.DefaultValue:
                        value = defaultValue;
                        return true;
                    case Fallback.Language:
                        if (TryGetValueWithLanguageFallback(content, alias, culture, segment, defaultValue, out value))
                            return true;
                        break;
                    case Fallback.Ancestors:
                        if (TryGetValueWithRecursiveFallback(content, alias, culture, segment, defaultValue, out value))
                            return true;
                        break;
                    default:
                        throw NotSupportedFallbackMethod(f, "content");
                }
            }

            value = defaultValue;
            return false;
        }

        private NotSupportedException NotSupportedFallbackMethod(int fallback, string level)
        {
            return new NotSupportedException($"Fallback {GetType().Name} does not support fallback code '{fallback}' at {level} level.");
        }

        // tries to get a value, recursing the tree
        private static bool TryGetValueWithRecursiveFallback<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, out T value)
        {
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

        // tries to get a value, falling back onto other languages
        private bool TryGetValueWithLanguageFallback<T>(IPublishedProperty property, string culture, string segment, T defaultValue, out T value)
        {
            value = defaultValue;

            if (culture.IsNullOrWhiteSpace()) return false;

            var visited = new HashSet<int>();

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language == null) return false;

            while (true)
            {
                if (language.FallbackLanguageId == null) return false;

                var language2Id = language.FallbackLanguageId.Value;
                if (visited.Contains(language2Id)) return false;
                visited.Add(language2Id);

                var language2 = _localizationService.GetLanguageById(language2Id);
                if (language2 == null) return false;
                var culture2 = language2.IsoCode;

                if (property.HasValue(culture2, segment))
                {
                    value = property.Value<T>(culture2, segment);
                    return true;
                }

                language = language2;
            }
        }

        // tries to get a value, falling back onto other languages
        private bool TryGetValueWithLanguageFallback<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, out T value)
        {
            value = defaultValue;

            if (culture.IsNullOrWhiteSpace()) return false;

            var visited = new HashSet<int>();

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language == null) return false;

            while (true)
            {
                if (language.FallbackLanguageId == null) return false;

                var language2Id = language.FallbackLanguageId.Value;
                if (visited.Contains(language2Id)) return false;
                visited.Add(language2Id);

                var language2 = _localizationService.GetLanguageById(language2Id);
                if (language2 == null) return false;
                var culture2 = language2.IsoCode;

                if (content.HasValue(alias, culture2, segment))
                {
                    value = content.Value<T>(alias, culture2, segment);
                    return true;
                }

                language = language2;
            }
        }

        // tries to get a value, falling back onto other languages
        private bool TryGetValueWithLanguageFallback<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, out T value)
        {
            value = defaultValue;

            if (culture.IsNullOrWhiteSpace()) return false;

            var visited = new HashSet<int>();

            // fixme
            // _localizationService.GetXxx() is expensive, it deep clones objects
            // we want _localizationService.GetReadOnlyXxx() returning IReadOnlyLanguage which cannot be saved back = no need to clone

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language == null) return false;

            while (true)
            {
                if (language.FallbackLanguageId == null) return false;

                var language2Id = language.FallbackLanguageId.Value;
                if (visited.Contains(language2Id)) return false;
                visited.Add(language2Id);

                var language2 = _localizationService.GetLanguageById(language2Id);
                if (language2 == null) return false;
                var culture2 = language2.IsoCode;

                if (content.HasValue(alias, culture2, segment))
                {
                    value = content.Value<T>(alias, culture2, segment);
                    return true;
                }

                language = language2;
            }
        }
    }
}
