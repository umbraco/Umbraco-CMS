using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models.Blocks;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection of block list items.
    /// </summary>
    public static class PartialsBlockListItemExtensions
    {
        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath = null, ViewDataDictionary viewData = null)
            => htmlHelper.Partials(blockListItems, blockListItem => GetPartialViewName(blockListItem, partialViewPath), viewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath, Func<BlockListItem, ViewDataDictionary> getViewData)
            => htmlHelper.Partials(blockListItems, blockListItem => GetPartialViewName(blockListItem, partialViewPath), getViewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items used as model for the partial views.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath, Func<BlockListItem, int, ViewDataDictionary> getViewData)
            => htmlHelper.Partials(blockListItems, (blockListItem, index) => GetPartialViewName(blockListItem, partialViewPath), getViewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model and the content type alias as partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, Func<BlockListItem, T> getModel, string partialViewPath, ViewDataDictionary viewData = null)
            => htmlHelper.Partials(blockListItems, getModel, (blockListItem, model, index) => GetPartialViewName(blockListItem, partialViewPath), (blockListItem, model, index) => viewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item as an HTML-encoded string, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items used as model for the partial views.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, Func<BlockListItem, T> getModel, string partialViewPath, Func<BlockListItem, T, int, ViewDataDictionary> getViewData)
            => htmlHelper.Partials(blockListItems, getModel, (blockListItem, model, index) => GetPartialViewName(blockListItem, partialViewPath), getViewData);

        /// <summary>
        /// Gets the partial view name for the <see cref="BlockListItem" />.
        /// </summary>
        /// <param name="blockListItem">The block list item.</param>
        /// <param name="partialViewPath">The partial view path.</param>
        /// <returns>
        /// The partial view name.
        /// </returns>
        internal static string GetPartialViewName(BlockListItem blockListItem, string partialViewPath) => PartialsPublishedElementExtensions.GetPartialViewName(blockListItem.Content, partialViewPath);
    }
}
