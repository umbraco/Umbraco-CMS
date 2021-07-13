using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection.
    /// </summary>
    public static class RenderPartialsExtensions
    {
        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, ViewDataDictionary viewData = null)
            => htmlHelper.RenderPartials(items, item => item, (item, model, index) => getPartialViewName(item), (item, model, index) => viewData);

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, Func<T, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(items, item => item, (item, model, index) => getPartialViewName(item), (item, model, index) => getViewData(item));

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> getPartialViewName, Func<T, int, ViewDataDictionary> getViewData)
            => htmlHelper.RenderPartials(items, item => item, (item, model, index) => getPartialViewName(item, index), (item, model, index) => getViewData(item, index));

        /// <summary>
        /// Renders the partial views for every <typeparamref name="TItem" /> item, using <paramref name="getModel" /> to get the model, <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <exception cref="System.ArgumentNullException">getModel
        /// or
        /// getPartialViewName
        /// or
        /// getViewData</exception>
        public static void RenderPartials<TItem, TModel>(this HtmlHelper htmlHelper, IEnumerable<TItem> items, Func<TItem, TModel> getModel, Func<TItem, TModel, int, string> getPartialViewName, Func<TItem, TModel, int, ViewDataDictionary> getViewData)
        {
            if (getModel == null) throw new ArgumentNullException(nameof(getModel));
            if (getPartialViewName == null) throw new ArgumentNullException(nameof(getPartialViewName));
            if (getViewData == null) throw new ArgumentNullException(nameof(getViewData));

            if (items != null)
            {
                var index = 0;
                foreach (var item in items)
                {
                    var model = getModel(item);
                    var partialViewName = getPartialViewName(item, model, index);
                    if (!string.IsNullOrEmpty(partialViewName))
                    {
                        htmlHelper.RenderPartial(partialViewName, model, getViewData(item, model, index) ?? htmlHelper.ViewData);
                    }

                    index++;
                }
            }
        }
    }
}
