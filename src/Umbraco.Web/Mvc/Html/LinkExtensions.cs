using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering link/anchor tags.
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// Returns a link/anchor tag by using the specified <paramref name="link" />, <paramref name="text" /> and <paramref name="htmlAttributes" />.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="link">The link.</param>
        /// <param name="text">The text of the link.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <remarks>
        /// The link is not rendered when <paramref name="link" /> is <c>null</c>.
        /// </remarks>
        /// <example>
        /// @Html.Link(Model.Button, htmlAttributes: new { @class = "button" })
        /// </example>
        public static IHtmlString Link(this HtmlHelper htmlHelper, Link link, string text = null, object htmlAttributes = null)
        {
            if (link == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                text = link.Name;
            }

            return htmlHelper.Link(link, _ => new HtmlString(HttpUtility.HtmlEncode(text)), htmlAttributes);
        }

        /// <summary>
        /// Returns a link/anchor tag by using the specified <paramref name="link" />, <paramref name="template" /> and <paramref name="htmlAttributes" />.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="link">The link.</param>
        /// <param name="template">The template.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <remarks>
        /// The link is not rendered when <paramref name="link" /> is <c>null</c>.
        /// </remarks>
        /// <example>
        /// @Html.Link(Model.Button, @<text><i class="fab fa-umbraco"></i> @item.Name</text>, new { @class = "button" })
        /// </example>
        public static IHtmlString Link(this HtmlHelper htmlHelper, Link link, Func<Link, IHtmlString> template, object htmlAttributes = null)
        {
            if (link == null)
            {
                return null;
            }

            return htmlHelper.Link(link.Url, _ => template(link), GetAttributes(htmlAttributes, link));
        }

        /// <summary>
        /// Returns a link/anchor tag by using the specified <paramref name="href" />, <paramref name="text" /> and <paramref name="htmlAttributes" />.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="href">The URL of the link.</param>
        /// <param name="text">The text of the link.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <example>
        /// @Html.Link(Model.Url, "Example text", new { @class = "button" })
        /// </example>
        public static IHtmlString Link(this HtmlHelper htmlHelper, string href, string text, object htmlAttributes = null)
        {
            return htmlHelper.Link(href, _ => new HtmlString(HttpUtility.HtmlEncode(text)), htmlAttributes);
        }

        /// <summary>
        /// Returns a link/anchor tag by using the specified <paramref name="href" />, <paramref name="template" /> and <paramref name="htmlAttributes" />.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="href">The URL of the link.</param>
        /// <param name="template">The template.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>
        /// The link/anchor tag.
        /// </returns>
        /// <example>
        /// @Html.Link(Model.Url, @<text><i class="fab fa-umbraco"></i> Example text</text>, new { @class = "button" })
        /// </example>
        public static IHtmlString Link(this HtmlHelper htmlHelper, string href, Func<string, IHtmlString> template, object htmlAttributes = null)
        {
            var linkTag = new TagBuilder("a")
            {
                InnerHtml = template(href).ToHtmlString()
            };

            var htmlAttributesDictionary = GetAttributes(htmlAttributes, href);
            linkTag.MergeAttributes(htmlAttributesDictionary);

            return htmlHelper.Raw(linkTag.ToString());
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <param name="link">The link.</param>
        /// <returns>
        /// The merged HTML attributes.
        /// </returns>
        internal static IDictionary<string, object> GetAttributes(object htmlAttributes, Link link)
        {
            var htmlAttributesDictionary = htmlAttributes as IDictionary<string, object> ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (!htmlAttributesDictionary.ContainsKey("target") &&
                link.Target is var linkTarget &&
                !string.IsNullOrWhiteSpace(linkTarget))
            {
                htmlAttributesDictionary["target"] = linkTarget;
            }

            return htmlAttributesDictionary;
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <param name="href">The href.</param>
        /// <returns>
        /// The merged HTML attributes.
        /// </returns>
        internal static IDictionary<string, object> GetAttributes(object htmlAttributes, string href)
        {
            var htmlAttributesDictionary = htmlAttributes as IDictionary<string, object> ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (!htmlAttributesDictionary.ContainsKey("rel") &&
                htmlAttributesDictionary.TryGetValue("target", out var target) &&
                target != null &&
                target.ToString().Equals("_blank", StringComparison.OrdinalIgnoreCase))
            {
                htmlAttributesDictionary["rel"] = "noopener";
            }

            if (!string.IsNullOrWhiteSpace(href))
            {
                htmlAttributesDictionary["href"] = href;
            }

            htmlAttributesDictionary.RemoveAll(kvp => kvp.Value == null);

            return htmlAttributesDictionary;
        }
    }
}
