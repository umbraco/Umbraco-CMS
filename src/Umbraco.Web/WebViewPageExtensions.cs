using System;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Umbraco.Web
{
    public static class WebViewPageExtensions
    {
        public static HelperResult RenderSection(this WebPageBase webPage, string name, Func<dynamic, HelperResult> defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : defaultContents(null);
        }

        public static HelperResult RenderSection(this WebPageBase webPage, string name, HelperResult defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : defaultContents;
        }

        public static HelperResult RenderSection(this WebPageBase webPage, string name, string defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : new HelperResult(text => text.Write(defaultContents));
        }

        public static HelperResult RenderSection(this WebPageBase webPage, string name, IHtmlString defaultContents)
        {
            return webPage.IsSectionDefined(name) ? webPage.RenderSection(name) : new HelperResult(text => text.Write(defaultContents));
        }
    }
}
