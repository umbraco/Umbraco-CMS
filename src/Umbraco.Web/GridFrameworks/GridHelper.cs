using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.GridFrameworks
{
    public static class GridHelper
    {
        /// <summary>
        /// Gets the div wrapper.
        /// </summary>
        /// <param name="cssClassName">Name of the CSS class.</param>
        /// <returns></returns>
        public static HtmlTagWrapper GetDivWrapper(string cssClassName)
        {
            return new HtmlTagWrapper("div").AddClassName(cssClassName);
        }

        /// <summary>
        /// Gets the div wrapper.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        public static HtmlTagWrapper GetDivWrapper(IGridAttributes attributes)
        {
            var tagWrapper = new HtmlTagWrapper("div");
            var attrs = attributes.GetAttributes();
            if (attrs.Any())
            {
                tagWrapper.ReflectAttributesFromAnonymousType(attrs);
            }
            return tagWrapper;
        }
    }
}