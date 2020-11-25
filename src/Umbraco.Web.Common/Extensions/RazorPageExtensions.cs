using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Umbraco.Extensions
{
    public static class RazorPageExtensions
    {
        public static HtmlString RenderSection(this RazorPage webPage, string name, HtmlString defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : defaultContents;
        }

        public static HtmlString RenderSection(this RazorPage webPage, string name, string defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : new HtmlString(defaultContents);
        }

    }
}
