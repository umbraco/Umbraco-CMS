using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection of published elements.
    /// </summary>
    public static class PartialsPublishedElementExtensions
    {
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
            => htmlHelper.Partials(elements, element => GetPartialViewName(element, partialViewPath), viewData);

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
            => htmlHelper.Partials(elements, element => GetPartialViewName(element, partialViewPath), getViewData);

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
            => htmlHelper.Partials(elements, (element, index) => GetPartialViewName(element, partialViewPath), getViewData);

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
            => htmlHelper.Partials(elements, getModel, (element, model, index) => GetPartialViewName(element, partialViewPath), (element, model, index) => viewData);

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
            => htmlHelper.Partials(elements, getModel, (element, model, index) => GetPartialViewName(element, partialViewPath), getViewData);

        /// <summary>
        /// Gets the partial view name for the <see cref="IPublishedElement" />.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="partialViewPath">The partial view path.</param>
        /// <returns>
        /// The partial view name.
        /// </returns>
        internal static string GetPartialViewName(IPublishedElement element, string partialViewPath) => (string.IsNullOrEmpty(partialViewPath) ? null : partialViewPath.EnsureEndsWith('/')) + element.ContentType.Alias;
    }
}
