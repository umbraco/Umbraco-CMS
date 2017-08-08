using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.GridFrameworks
{
    internal static class GridHelper
    {
        /// <summary>
        /// Gets the div wrapper.
        /// </summary>
        /// <param name="cssClassName">Name of the CSS class.</param>
        /// <returns></returns>
        internal static HtmlTagWrapper GetDivWrapper(string cssClassName)
        {
            var div = new HtmlTagWrapper("div");
            div.AddClassName(cssClassName);
            return div;
        }

        /// <summary>
        /// Gets the div wrapper.
        /// </summary>
        /// <param name="element">The attributes.</param>
        /// <returns></returns>
        internal static HtmlTagWrapper GetDivWrapper(IGridAttributes element)
        {
            var tagWrapper = new HtmlTagWrapper("div");
            var attr = element.GetAttributes();
            if (attr.Any())
            {
                tagWrapper.ReflectAttributesFromAnonymousType(element);
            }
            return tagWrapper;
        }
    }
}