using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    public static partial class HtmlHelperPartialsExtensions
    {
        /// <summary>
        /// Gets the partial view name for <see cref="IPublishedElement" /> items.
        /// </summary>
        private static readonly Func<IPublishedElement, string, string> getElementPartialViewName = (c, prefix) => (string.IsNullOrEmpty(prefix) ? null : prefix.EnsureEndsWith('/')) + c.ContentType.Alias;

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item as an HTML-encoded string, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath = null, ViewDataDictionary viewData = null)
        {
            return htmlHelper.Partials(elements, c => getElementPartialViewName(c, partialViewPath), viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(elements, c => getElementPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, int, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(elements, (c, i) => getElementPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model and the content type alias as partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, Func<IPublishedElement, T> getModel, string partialViewPath, ViewDataDictionary viewData = null)
        {
            return htmlHelper.Partials(elements, getModel, (c, m, i) => getElementPartialViewName(c, partialViewPath), (c, m, i) => viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, Func<IPublishedElement, T> getModel, string partialViewPath, Func<IPublishedElement, T, int, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(elements, getModel, (c, m, i) => getElementPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath = null, ViewDataDictionary viewData = null)
        {
            htmlHelper.RenderPartials(elements, c => getElementPartialViewName(c, partialViewPath), viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(elements, c => getElementPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, int, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(elements, (c, i) => getElementPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, Func<IPublishedElement, T> getModel, string partialViewPath, Func<IPublishedElement, T, int, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(elements, getModel, (c, m, i) => getElementPartialViewName(c, partialViewPath), getViewData);
        }
    }
}
