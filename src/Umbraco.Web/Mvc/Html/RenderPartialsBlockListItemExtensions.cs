using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core.Models.Blocks;
using static Umbraco.Web.Mvc.Html.PartialsBlockListItemExtensions;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection of block list items.
    /// </summary>
    public static class RenderPartialsBlockListItemExtensions
    {
        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath = null, ViewDataDictionary viewData = null)
            => htmlHelper.RenderPartials(blockListItems, blockListItem => GetPartialViewName(blockListItem, partialViewPath), viewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath, Func<BlockListItem, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(blockListItems, blockListItem => GetPartialViewName(blockListItem, partialViewPath), getViewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items to render partial views for.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, string partialViewPath, Func<BlockListItem, int, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(blockListItems, (blockListItem, index) => GetPartialViewName(blockListItem, partialViewPath), getViewData);

        /// <summary>
        /// Renders the partial views for every <see cref="BlockListItem" /> item, using <paramref name="getModel" /> to get the model, the content type alias as partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="blockListItems">The block list items to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="partialViewPath">The partial view path, prefixed to the content type alias.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<BlockListItem> blockListItems, Func<BlockListItem, T> getModel, string partialViewPath, Func<BlockListItem, T, int, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(blockListItems, getModel, (blockListItem, model, index) => GetPartialViewName(blockListItem, partialViewPath), getViewData);
    }
}
