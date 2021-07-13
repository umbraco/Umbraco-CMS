using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using static Umbraco.Web.Mvc.Html.PartialsPublishedElementExtensions;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection of published elements.
    /// </summary>
    public static class RenderPartialsPublishedElementExtensions
    {
        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath = null, ViewDataDictionary viewData = null)
            => htmlHelper.RenderPartials(elements, element => GetPartialViewName(element, partialViewPath), viewData);

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(elements, element => GetPartialViewName(element, partialViewPath), getViewData);

        /// <summary>
        /// Renders the partial views for every <see cref="IPublishedElement" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="elements">The elements to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<IPublishedElement> elements, string partialViewPath, Func<IPublishedElement, int, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(elements, (element, index) => GetPartialViewName(element, partialViewPath), getViewData);

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
            => htmlHelper.RenderPartials(elements, getModel, (element, model, index) => GetPartialViewName(element, partialViewPath), getViewData);
    }
}
