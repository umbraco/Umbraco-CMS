using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedValueFallback"/> that allows
    /// for use of fall-back languages
    /// </summary>
    /// <remarks>
    /// Inherits from <see cref="PublishedValueFallback" /> that implments what was available in v7.
    /// </remarks>
    public class PublishedValueLanguageFallback : PublishedValueFallback
    {
        private readonly ILocalizationService _localizationService;

        public PublishedValueLanguageFallback(ServiceContext services)
        {
            _localizationService = services.LocalizationService;
        }

        /// <inheritdoc />
        public override object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages)
        {
            object value;
            if (TryGetValueFromFallbackLanguage(property, culture, segment, defaultValue, visitedLanguages, out value))
            {
                return value;
            }

            return base.GetValue(property, culture, segment, defaultValue, visitedLanguages);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages)
        {
            T value;
            if (TryGetValueFromFallbackLanguage(property, culture, segment, defaultValue, visitedLanguages, out value))
            {
                return value;
            }

            return base.GetValue(property, culture, segment, defaultValue, visitedLanguages);
        }

        /// <inheritdoc />
        public override object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue, ICollection<int> visitedLanguages)
        {
            object value;
            if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, visitedLanguages, out value))
            {
                return value;
            }

            return base.GetValue(content, alias, culture, segment, defaultValue, visitedLanguages);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages)
        {
            T value;
            if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, visitedLanguages, out value))
            {
                return value;
            }

            return base.GetValue(content, alias, culture, segment, defaultValue, visitedLanguages);
        }

        /// <inheritdoc />
        public override object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority, ICollection<int> visitedLanguages)
        {
            return GetValue<object>(content, alias, culture, segment, defaultValue, recurse, fallbackPriority, visitedLanguages);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority, ICollection<int> visitedLanguages)
        {
            if (fallbackPriority == PublishedValueFallbackPriority.RecursiveTree)
            {
                var result = base.GetValue<T>(content, alias, culture, segment, defaultValue, recurse, PublishedValueFallbackPriority.RecursiveTree, visitedLanguages);
                if (ValueIsNotNullEmptyOrDefault(result, defaultValue))
                {
                    // We've prioritised recursive tree search and found a value, so can return it.
                    return result;
                }

                if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, recurse, fallbackPriority, visitedLanguages, out result))
                {
                    return result;
                }

                return defaultValue;
            }

            if (fallbackPriority == PublishedValueFallbackPriority.FallbackLanguage)
            {
                T result;
                if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, recurse, fallbackPriority, visitedLanguages, out result))
                {
                    return result;
                }
            }

            // No language fall back content found, so use base implementation
            return base.GetValue<T>(content, alias, culture, segment, defaultValue, recurse, fallbackPriority, visitedLanguages);
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedProperty property, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages, out T value)
        {
            value = defaultValue;

            if (string.IsNullOrEmpty(culture))
            {
                return false;
            }

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguageId.HasValue == false)
            {
                return false;
            }

            if (AlreadyVisitedLanguage(visitedLanguages, language.FallbackLanguageId.Value))
            {
                return false;
            }

            visitedLanguages.Add(language.FallbackLanguageId.Value);

            var fallbackLanguage = GetLanguageById(language.FallbackLanguageId.Value);
            value = property.Value(fallbackLanguage.IsoCode, segment, defaultValue, visitedLanguages);
            if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
            {
                return true;
            }

            return false;
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, ICollection<int> visitedLanguages, out T value)
        {
            value = defaultValue;

            if (string.IsNullOrEmpty(culture))
            {
                return false;
            }

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguageId.HasValue == false)
            {
                return false;
            }

            if (AlreadyVisitedLanguage(visitedLanguages, language.FallbackLanguageId.Value))
            {
                return false;
            }

            visitedLanguages.Add(language.FallbackLanguageId.Value);

            var fallbackLanguage = GetLanguageById(language.FallbackLanguageId.Value);
            value = content.Value(alias, fallbackLanguage.IsoCode, segment, defaultValue, visitedLanguages);
            if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
            {
                return true;
            }

            return false;
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority, ICollection<int> visitedLanguages, out T value)
        {
            value = defaultValue;
            if (string.IsNullOrEmpty(culture))
            {
                return false;
            }

            var language = _localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguageId.HasValue == false)
            {
                return false;
            }

            if (AlreadyVisitedLanguage(visitedLanguages, language.FallbackLanguageId.Value))
            {
                return false;
            }

            visitedLanguages.Add(language.FallbackLanguageId.Value);

            var fallbackLanguage = GetLanguageById(language.FallbackLanguageId.Value);
            value = content.Value(alias, fallbackLanguage.IsoCode, segment, defaultValue, recurse, fallbackPriority, visitedLanguages);
            if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
            {
                return true;
            }

            return false;
        }

        private static bool AlreadyVisitedLanguage(ICollection<int> visitedLanguages, int fallbackLanguageId)
        {
            return visitedLanguages.Contains(fallbackLanguageId);
        }

        private ILanguage GetLanguageById(int id)
        {
            return _localizationService.GetLanguageById(id);
        }

        private static bool ValueIsNotNullEmptyOrDefault<T>(T value, T defaultValue)
        {
            return value != null &&
                   string.IsNullOrEmpty(value.ToString()) == false &&
                   value.Equals(defaultValue) == false;
        }
    }
}
