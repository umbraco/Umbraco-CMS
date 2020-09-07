using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.Blocks;

namespace Umbraco.Web
{
    public static partial class HtmlHelperPartialsExtensions
    {
        /// <summary>
        /// Gets the partial view name for <see cref="BlockListItem" /> items.
        /// </summary>
        private static readonly Func<BlockListItem, string, string> getBlockListItemPartialViewName = (c, prefix) => getElementPartialViewName(c.Content, prefix);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath = null, ViewDataDictionary viewData = null)
        {
            return htmlHelper.Partials(blocks, c => getBlockListItemPartialViewName(c, partialViewPath), viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath, Func<BlockListItem, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(blocks, c => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath, Func<BlockListItem, int, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(blocks, (c, i) => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model and the content type alias as partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, Func<BlockListItem, T> getModel, string partialViewPath, ViewDataDictionary viewData = null)
        {
            return htmlHelper.Partials(blocks, getModel, (c, m, i) => getBlockListItemPartialViewName(c, partialViewPath), (c, m, i) => viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, Func<BlockListItem, T> getModel, string partialViewPath, Func<BlockListItem, T, int, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(blocks, getModel, (c, m, i) => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath = null, ViewDataDictionary viewData = null)
        {
            htmlHelper.RenderPartials(blocks, c => getBlockListItemPartialViewName(c, partialViewPath), viewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath, Func<BlockListItem, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(blocks, c => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, string partialViewPath, Func<BlockListItem, int, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(blocks, (c, i) => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blocks">The blocks to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blocks, Func<BlockListItem, T> getModel, string partialViewPath, Func<BlockListItem, T, int, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(blocks, getModel, (c, m, i) => getBlockListItemPartialViewName(c, partialViewPath), getViewData);
        }
    }
}
