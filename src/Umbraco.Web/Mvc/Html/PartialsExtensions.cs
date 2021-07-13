using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Umbraco.Web.Mvc.Html
{
    /// <summary>
    /// Provides support for rendering partial views from a collection.
    /// </summary>
    public static class PartialsExtensions
    {
        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, ViewDataDictionary viewData = null)
            => htmlHelper.Partials(items, item => item, (item, model, index) => getPartialViewName(item), (item, model, index) => viewData);

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, Func<T, ViewDataDictionary> getViewData)
            => htmlHelper.Partials(items, item => item, (item, model, index) => getPartialViewName(item), (item, model, index) => getViewData(item));

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> getPartialViewName, Func<T, int, ViewDataDictionary> getViewData)
            => htmlHelper.Partials(items, item => item, (item, model, index) => getPartialViewName(item, index), (item, model, index) => getViewData(item, index));

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">getModel
        /// or
        /// getPartialViewName
        /// or
        /// getViewData</exception>
        public static IHtmlString Partials<TItem, TModel>(this HtmlHelper htmlHelper, IEnumerable<TItem> items, Func<TItem, TModel> getModel, Func<TItem, TModel, int, string> getPartialViewName, Func<TItem, TModel, int, ViewDataDictionary> getViewData)
        {
            if (getModel == null) throw new ArgumentNullException(nameof(getModel));
            if (getPartialViewName == null) throw new ArgumentNullException(nameof(getPartialViewName));
            if (getViewData == null) throw new ArgumentNullException(nameof(getViewData));

            var sb = new StringBuilder();

            if (items != null)
            {
                var index = 0;
                foreach (var item in items)
                {
                    var model = getModel(item);
                    var partialViewName = getPartialViewName(item, model, index);
                    if (!string.IsNullOrEmpty(partialViewName))
                    {
                        sb.Append(htmlHelper.Partial(partialViewName, model, getViewData(item, model, index) ?? htmlHelper.ViewData));
                    }

                    index++;
                }
            }

            return new HtmlString(sb.ToString());
        }
    }
}
