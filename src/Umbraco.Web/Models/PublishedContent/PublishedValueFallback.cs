using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using ValueFallback = Umbraco.Core.Constants.Content.ValueFallback;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedValueFallback"/>.
    /// </summary>
    public class PublishedValueFallback : IPublishedValueFallback
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedValueFallback"/> class.
        /// </summary>
        /// <param name="serviceContext"></param>
        public PublishedValueFallback(ServiceContext serviceContext)
        {
            _localizationService = serviceContext.LocalizationService;
        }

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
        public object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, int fallback)
        {
            // is that ok?
            return GetValue<object>(content, alias, culture, segment, defaultValue, fallback);
        }

        /// <inheritdoc />
        public virtual T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, int fallback)
        {
            switch (fallback)
            {
                case ValueFallback.None:
                case ValueFallback.Default:
                    return defaultValue;
                case ValueFallback.Recurse:
                    return TryGetValueWithRecursiveFallback(content, alias, culture, segment, defaultValue, out var value1) ? value1 : defaultValue;
                case ValueFallback.Language:
                    return TryGetValueWithLanguageFallback(content, alias, culture, segment, defaultValue, out var value2) ? value2 : defaultValue;
                case ValueFallback.RecurseThenLanguage:
                    return TryGetValueWithRecursiveFallback(content, alias, culture, segment, defaultValue, out var value3)
                        ? value3
                        : TryGetValueWithLanguageFallback(content, alias, culture, segment, defaultValue, out var value4) ? value4 : defaultValue;
                case ValueFallback.LanguageThenRecurse:
                    return TryGetValueWithLanguageFallback(content, alias, culture, segment, defaultValue, out var value5)
                        ? value5
                        : TryGetValueWithRecursiveFallback(content, alias, culture, segment, defaultValue, out var value6) ? value6 : defaultValue;
                default:
                    throw new NotSupportedException($"Fallback {GetType().Name} does not support policy code '{fallback}'.");
            }
        }

        // tries to get a value, recursing the tree
        protected static bool TryGetValueWithRecursiveFallback<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, out T value)
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
