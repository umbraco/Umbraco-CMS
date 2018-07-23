using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override bool TryGetValueWithFallbackMethod<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages, int fallbackMethod, out T value)
        {
            value = defaultValue;
            switch (fallbackMethod)
            {
                case Core.Constants.Content.FallbackMethods.None:
                    return false;
                case Core.Constants.Content.FallbackMethods.RecursiveTree:
                    return TryGetValueWithRecursiveTree(content, alias, culture, segment, defaultValue, out value);
                case Core.Constants.Content.FallbackMethods.FallbackLanguage:
                    return TryGetValueWithFallbackLanguage(content, alias, culture, segment, defaultValue, fallbackMethods, visitedLanguages, out value);
                default:
                    throw new NotSupportedException($"Fallback method with indentifying number {fallbackMethod} is not supported within {GetType().Name}.");
            }
        }

        private bool TryGetValueWithFallbackLanguage<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, IEnumerable<int> fallbackMethods, ICollection<int> visitedLanguages, out T value)
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
            value = content.Value(alias, fallbackLanguage.IsoCode, segment, defaultValue, fallbackMethods.ToArray(), visitedLanguages);
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
