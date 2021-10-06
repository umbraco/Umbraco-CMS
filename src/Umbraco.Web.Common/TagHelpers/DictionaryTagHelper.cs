using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Dictionary;

namespace Umbraco.Cms.Web.Common.TagHelpers
{
    [HtmlTargetElement("umb-dictionary")]
    public class DictionaryTagHelper : TagHelper
    {
        private readonly ICultureDictionaryFactory _cultureDictionaryFactory;
        private ICultureDictionary _cultureDictionary;

        public DictionaryTagHelper(ICultureDictionaryFactory cultureDictionaryFactory)
        {
            _cultureDictionaryFactory = cultureDictionaryFactory;
        }

        [HtmlAttributeNotBound]
        public ICultureDictionary CultureDictionary => _cultureDictionary ??= _cultureDictionaryFactory.CreateDictionary();

        [HtmlAttributeName("key")]
        public string Key { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = ""; // Remove the outer tag of <umb-dictionary>

            // Ensure we have a dictionary key to lookup
            if(string.IsNullOrEmpty(Key) == false)
            {
                var localizedValue = CultureDictionary[Key];
                if (string.IsNullOrEmpty(localizedValue) == false)
                {
                    // Only replace the HTML inside the <umb-dictionary> tag if we have a value
                    output.Content.SetHtmlContent(localizedValue);
                }
            }
        }
    }
}
