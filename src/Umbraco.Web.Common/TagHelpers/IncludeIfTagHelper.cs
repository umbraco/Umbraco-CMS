using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Umbraco.Cms.Web.Common.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "umb-include-if")]
    public class IncludeIfTagHelper : TagHelper
    {
        [HtmlAttributeName("umb-include-if")]
        public bool Predicate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Predicate)
            {
                output.SuppressOutput();
            }
        }
    }
}
