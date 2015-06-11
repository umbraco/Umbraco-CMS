using System.Web.Mvc;
using System.Web.UI;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Minifies the result for the JavaScriptResult
    /// </summary>
    /// <remarks>
    /// Only minifies in release mode
    /// </remarks>
    public class MinifyJavaScriptResultAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Minify the result if in release mode
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            if (filterContext.Result == null) return;
            var jsResult = filterContext.Result as JavaScriptResult;
            if (jsResult == null) return;
            if (filterContext.HttpContext.IsDebuggingEnabled) return;
            
            //minify the result
            var result = jsResult.Script;
            var minifier = new JSMin();
            var minified = minifier.Minify(result);            
            jsResult.Script = minified;
        }

    }
}