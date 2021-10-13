using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Stubble.Core.Builders;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.TagHelpers
{
    /// <summary>
    /// Use <umb-lang-switcher /> for a simple unordered list of Domain/Culture links
    /// </summary>
    [HtmlTargetElement("umb-lang-switcher")]
    public class LanguageSwitcherTagHelper : TagHelper
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public LanguageSwitcherTagHelper(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = ""; // Remove the <umb-lang-switcher> tag

            // Attempt to get context
            if(_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                // Current Culture of the page
                var currentCulture = umbracoContext.PublishedRequest.Culture;

                // Current Page/Content node used
                var currentNode = umbracoContext.PublishedRequest.PublishedContent;

                // All domains in a site
                // NOTE: A culture/language could have multiple domains assigned (So use the first we find)
                var allDomains = umbracoContext.Domains.GetAll(false);
                var allDomainsFiltered = allDomains?.GroupBy(d => d.Culture).Select(g => g.First()).ToList();

                if (allDomainsFiltered.Count() > 0)
                {
                    // List of languages/model to use with
                    var data = new Dictionary<string, object>();
                    var languages = new List<LanguageSwitcherLang>();

                    //var unorderedItem = new TagBuilder("ul");

                    foreach (var domain in allDomainsFiltered)
                    {
                        // Will get the language such as English or Dansk without the (United States) country prefixed
                        var culture = new CultureInfo(domain.Culture);
                        var languageName = culture.IsNeutralCulture ? culture.NativeName : culture.Parent.NativeName;

                        var lang = new LanguageSwitcherLang
                        {
                            Name = languageName,
                            Culture = domain.Culture,
                            Url = domain.Name,
                            IsCurrentLang = domain.Culture == currentCulture
                        };

                        languages.Add(lang);

                        //var linkItem = new TagBuilder("a");
                        //if (domain.Culture == currentCulture)
                        //{
                        //    linkItem.AddCssClass("selected");
                        //}

                        //// add href="" to <a>
                        //linkItem.Attributes["href"] = domain.Name;

                        

                        //// Set the Native Name/Language of the culture in the <a>
                        //linkItem.InnerHtml.Append(language);

                        //// Add lang and hreflang attributes to <a>
                        //linkItem.Attributes["lang"] = domain.Culture;
                        //linkItem.Attributes["hreflang"] = domain.Culture;

                        //// Add <a> to <li>
                        //var listItem = new TagBuilder("li");
                        //listItem.InnerHtml.AppendHtml(linkItem);

                        //// Add <li> to <ul>
                        //unorderedItem.InnerHtml.AppendHtml(listItem);
                    }

                    data.Add("Languages", languages);

                    // Get HTML content inside taghelper
                    // This content inside is a HTML Mustache template
                    var content = await output.GetChildContentAsync();
                    var mustacheTemplate = content.GetContent();


                    var stubble = new StubbleBuilder().Build();
                    var rendered = stubble.Render(mustacheTemplate, data);

                    output.Content.SetHtmlContent(rendered);
                }
                else
                {
                    // No domains so render nothing
                    output.SuppressOutput();
                }
            }
        }
    }

    public class LanguageSwitcherLang
    {
        public string Culture { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public bool IsCurrentLang { get; set; }
    }
}
