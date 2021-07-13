using System;
using System.Web.Mvc;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering a link/anchor with seperate start/end tags.
    /// </summary>
    public static class BeginLinkExtensions
    {
        /// <summary>
        /// Begins the link/anchor tag.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="link">The link.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <remarks>
        /// The link start and end tag is not rendered when <paramref name="link" /> is <c>null</c>.
        /// </remarks>
        /// <example>
        /// @using (Html.BeginLink(Model.Button, new { @class = "button" }))
        /// {
        ///     <i class="fab fa-umbraco"></i> Example text
        /// }
        /// </example>
        public static IDisposable BeginLink(this HtmlHelper htmlHelper, Link link, object htmlAttributes = null)
        {
            if (link == null)
            {
                return null;
            }

            return htmlHelper.BeginLink(link.Url, LinkExtensions.GetAttributes(htmlAttributes, link));
        }

        /// <summary>
        /// Begins the link/anchor tag.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="href">The URL of the link.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <example>
        /// @using (Html.BeginLink(Model.Url, new { @class = "button" }))
        /// {
        ///     <i class="fab fa-umbraco"></i> Example text
        /// }
        /// </example>
        public static IDisposable BeginLink(this HtmlHelper htmlHelper, string href, object htmlAttributes = null)
        {
            return new DisposableTagScope(htmlHelper.ViewContext, "a", LinkExtensions.GetAttributes(htmlAttributes, href)).Start();
        }
    }
}
