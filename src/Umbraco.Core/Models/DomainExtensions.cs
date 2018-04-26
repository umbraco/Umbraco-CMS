using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public static class DomainExtensions
    {
        /// <summary>
        /// Returns ture if the <see cref="IDomain"/> has a culture code equal to the default language specified
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localizationService"></param>
        /// <returns></returns>
        public static bool IsDefaultDomain(this IDomain domain, ILocalizationService localizationService)
        {
            var defaultLang = localizationService.GetDefaultVariantLanguage();
            if (defaultLang == null)
                return false; //if for some reason a null value is returned (i.e. no languages or based on mock unit test data), then assume false

            return domain.LanguageIsoCode == defaultLang.CultureName;
        }
    }
}
