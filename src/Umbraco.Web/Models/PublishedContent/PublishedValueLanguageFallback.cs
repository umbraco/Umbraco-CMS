using LightInject;
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
        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        [Inject]
        public ServiceContext Services { get; set; }

        /// <inheritdoc />
        public override object GetValue(IPublishedProperty property, string culture, string segment, object defaultValue)
        {
            object value;
            if (TryGetValueFromFallbackLanguage(property, culture, segment, defaultValue, out value))
            {
                return value;
            }

            return base.GetValue(property, culture, segment, defaultValue);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedProperty property, string culture, string segment, T defaultValue)
        {
            T value;
            if (TryGetValueFromFallbackLanguage(property, culture, segment, defaultValue, out value))
            {
                return value;
            }

            return base.GetValue(property, culture, segment, defaultValue);
        }

        /// <inheritdoc />
        public override object GetValue(IPublishedElement content, string alias, string culture, string segment, object defaultValue)
        {
            object value;
            if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, out value))
            {
                return value;
            }

            return base.GetValue(content, alias, culture, segment, defaultValue);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue)
        {
            T value;
            if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, out value))
            {
                return value;
            }

            return base.GetValue(content, alias, culture, segment, defaultValue);
        }

        /// <inheritdoc />
        public override object GetValue(IPublishedContent content, string alias, string culture, string segment, object defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority)
        {
            return GetValue<object>(content, alias, culture, segment, defaultValue, recurse, fallbackPriority);
        }

        /// <inheritdoc />
        public override T GetValue<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, PublishedValueFallbackPriority fallbackPriority)
        {
            if (fallbackPriority == PublishedValueFallbackPriority.RecursiveTree)
            {
                var result = base.GetValue<T>(content, alias, culture, segment, defaultValue, recurse, PublishedValueFallbackPriority.RecursiveTree);
                if (ValueIsNotNullEmptyOrDefault(result, defaultValue))
                {
                    // We've prioritised recursive tree search and found a value, so can return it.
                    return result;
                }

                if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, recurse, out result))
                {
                    return result;
                }

                return defaultValue;
            }

            if (fallbackPriority == PublishedValueFallbackPriority.FallbackLanguage)
            {
                T result;
                if (TryGetValueFromFallbackLanguage(content, alias, culture, segment, defaultValue, recurse, out result))
                {
                    return result;
                }
            }

            // No language fall back content found, so use base implementation
            return base.GetValue<T>(content, alias, culture, segment, defaultValue, recurse, fallbackPriority);
        }

        private static bool ValueIsNotNullEmptyOrDefault<T>(T value, T defaultValue)
        {
            return value != null &&
                string.IsNullOrEmpty(value.ToString()) == false &&
                value.Equals(defaultValue) == false;
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedProperty property, string culture, string segment, T defaultValue, out T value)
        {
            if (string.IsNullOrEmpty(culture))
            {
                value = defaultValue;
                return false;
            }

            var localizationService = Services.LocalizationService;
            var language = localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguage == null)
            {
                value = defaultValue;
                return false;
            }

            var fallbackLanguage = language.FallbackLanguage;
            while (fallbackLanguage != null)
            {
                value = property.Value(fallbackLanguage.IsoCode, segment, defaultValue);
                if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
                {
                    return true;
                }

                fallbackLanguage = GetNextFallbackLanguage(fallbackLanguage, localizationService);
            }

            value = defaultValue;
            return false;
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedElement content, string alias, string culture, string segment, T defaultValue, out T value)
        {
            if (string.IsNullOrEmpty(culture))
            {
                value = defaultValue;
                return false;
            }

            var localizationService = Services.LocalizationService;
            var language = localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguage == null)
            {
                value = defaultValue;
                return false;
            }

            var fallbackLanguage = language.FallbackLanguage;
            while (fallbackLanguage != null)
            {
                value = content.Value(alias, fallbackLanguage.IsoCode, segment, defaultValue);
                if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
                {
                    return true;
                }

                fallbackLanguage = GetNextFallbackLanguage(fallbackLanguage, localizationService);
            }

            value = defaultValue;
            return false;
        }

        private bool TryGetValueFromFallbackLanguage<T>(IPublishedContent content, string alias, string culture, string segment, T defaultValue, bool recurse, out T value)
        {
            if (string.IsNullOrEmpty(culture))
            {
                value = defaultValue;
                return false;
            }

            var localizationService = Services.LocalizationService;
            var language = localizationService.GetLanguageByIsoCode(culture);
            if (language.FallbackLanguage == null)
            {
                value = defaultValue;
                return false;
            }

            var fallbackLanguage = language.FallbackLanguage;
            while (fallbackLanguage != null)
            {
                value = content.Value(alias, fallbackLanguage.IsoCode, segment, defaultValue, recurse);
                if (ValueIsNotNullEmptyOrDefault(value, defaultValue))
                {
                    return true;
                }

                fallbackLanguage = GetNextFallbackLanguage(fallbackLanguage, localizationService);
            }

            value = defaultValue;
            return false;
        }

        private static ILanguage GetNextFallbackLanguage(ILanguage fallbackLanguage, ILocalizationService localizationService)
        {
            fallbackLanguage = localizationService.GetLanguageById(fallbackLanguage.Id); // Ensures reference to next fall-back language is loaded if it exists
            return fallbackLanguage.FallbackLanguage;
        }
    }
}
