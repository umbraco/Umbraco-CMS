using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Umbraco.Web
{
    public static partial class HtmlHelperPartialsExtensions
    {
        /// <summary>
        /// Renders the partial views for every <paramref name="items" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, ViewDataDictionary viewData = null)
        {
            return htmlHelper.Partials(items, c => c, (c, m, i) => getPartialViewName(c), (c, m, i) => viewData);
        }

        /// <summary>
        /// Renders the partial views for every <paramref name="items" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, Func<T, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(items, c => c, (c, m, i) => getPartialViewName(c), (c, m, i) => getViewData(c));
        }

        /// <summary>
        /// Renders the partial views for every <paramref name="items" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        /// <exception cref="ArgumentNullException">getPartialViewName
        /// or
        /// getViewData</exception>
        public static IHtmlString Partials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> getPartialViewName, Func<T, int, ViewDataDictionary> getViewData)
        {
            return htmlHelper.Partials(items, c => c, (c, m, i) => getPartialViewName(c, i), (c, m, i) => getViewData(c, i));
        }

        /// <summary>
        /// Renders the partial views for every <paramref name="items" /> item as an HTML-encoded string, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T1">The type of the items.</typeparam>
        /// <typeparam name="T2">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <returns>
        /// The partial views that are rendered as an HTML-encoded string.
        /// </returns>
        /// <exception cref="ArgumentNullException">getModel
        /// or
        /// getPartialViewName
        /// or
        /// getViewData</exception>
        public static IHtmlString Partials<T1, T2>(this HtmlHelper htmlHelper, IEnumerable<T1> items, Func<T1, T2> getModel, Func<T1, T2, int, string> getPartialViewName, Func<T1, T2, int, ViewDataDictionary> getViewData)
        {
            if (getModel == null)
            {
                throw new ArgumentNullException(nameof(getModel));
            }
            if (getPartialViewName == null)
            {
                throw new ArgumentNullException(nameof(getPartialViewName));
            }
            if (getViewData == null)
            {
                throw new ArgumentNullException(nameof(getViewData));
            }

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

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="viewData">The view data for the partial views.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, ViewDataDictionary viewData = null)
        {
            htmlHelper.RenderPartials(items, c => c, (c, m, i) => getPartialViewName(c), (c, m, i) => viewData);
        }

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, string> getPartialViewName, Func<T, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(items, c => c, (c, m, i) => getPartialViewName(c), (c, m, i) => getViewData(c));
        }

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T" /> item, using <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        public static void RenderPartials<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> getPartialViewName, Func<T, int, ViewDataDictionary> getViewData)
        {
            htmlHelper.RenderPartials(items, c => c, (c, m, i) => getPartialViewName(c, i), (c, m, i) => getViewData(c, i));
        }

        /// <summary>
        /// Renders the partial views for every <typeparamref name="T1" /> item, using <paramref name="getModel" /> to get the model, <paramref name="getPartialViewName" /> to get the partial view name and <paramref name="getViewData" /> to get the view data.
        /// </summary>
        /// <typeparam name="T1">The type of the items.</typeparam>
        /// <typeparam name="T2">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="items">The items to render partial views for.</param>
        /// <param name="getModel">A function to get the model for an item.</param>
        /// <param name="getPartialViewName">A function to get the partial view name for an item.</param>
        /// <param name="getViewData">A function to get the view data for an item.</param>
        /// <exception cref="ArgumentNullException">getModel
        /// or
        /// getPartialViewName
        /// or
        /// getViewData</exception>
        public static void RenderPartials<T1, T2>(this HtmlHelper htmlHelper, IEnumerable<T1> items, Func<T1, T2> getModel, Func<T1, T2, int, string> getPartialViewName, Func<T1, T2, int, ViewDataDictionary> getViewData)
        {
            if (getModel == null)
            {
                throw new ArgumentNullException(nameof(getModel));
            }
            if (getPartialViewName == null)
            {
                throw new ArgumentNullException(nameof(getPartialViewName));
            }
            if (getViewData == null)
            {
                throw new ArgumentNullException(nameof(getViewData));
            }

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
