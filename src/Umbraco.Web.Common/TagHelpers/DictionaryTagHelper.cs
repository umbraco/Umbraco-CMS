using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.TagHelpers
{
    /// <summary>
    /// Translates an Umbraco Dictionary Key for the current language
    /// or a fallback language and uses an element
    /// </summary>
    [HtmlTargetElement("umb-dictionary", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class DictionaryTagHelper : TagHelper
    {
        private readonly ILocalizationService _localizationService;

        public DictionaryTagHelper(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// The dictionary key to translate
        /// </summary>
        [HtmlAttributeName("key")]
        public string Key { get; set; }

        /// <summary>
        /// An optional attribute to set a fallback language to use
        /// If the current language does not contain a translation for the key
        /// </summary>
        [HtmlAttributeName("fallback-lang")]
        public string FallbackLang { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = ""; // Remove the outer tag of <umb-dictionary>

            // Ensure we have a dictionary key to lookup
            if(string.IsNullOrEmpty(Key) == false)
            {
                // Get current culture
                var currentCulture = CultureInfo.CurrentCulture;

                // Ensure the Dictionary item/key even exist
                var translation = _localizationService.GetDictionaryItemByKey(Key);
                if(translation != null)
                {
                    // Try to see if we have a value set for the current culture/language
                    var langTranslation = translation.Translations.FirstOrDefault(x => x.Language.CultureInfo.Name.Equals(currentCulture.Name, comparisonType: System.StringComparison.InvariantCultureIgnoreCase));
                    if(string.IsNullOrEmpty(langTranslation?.Value) == false)
                    {
                        // Only replace the HTML inside the <umb-dictionary> tag if we have a value
                        output.Content.SetHtmlContent(langTranslation.Value);
                    }

                    // If we can't find the current lang value - check if we have set an attribute to check for a fallback
                    else if(string.IsNullOrEmpty(FallbackLang) == false)
                    {
                        // Try & see if we have a value set for fallback lang
                        var fallbackLangTranslation = translation.Translations.FirstOrDefault(x => x.Language.CultureInfo.Name.Equals(FallbackLang, comparisonType: System.StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrEmpty(fallbackLangTranslation?.Value) == false)
                        {
                            // Only replace the HTML inside the <umb-dictionary> tag if we have a value for the fallback lang
                            output.Content.SetHtmlContent(fallbackLangTranslation.Value);
                        }
                    }
                }
            }
        }
    }
}
