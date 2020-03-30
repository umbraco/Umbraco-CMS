using System.Web.Mvc;
using Umbraco.Core.Assets;
using Umbraco.Web.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Runtime;

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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeMinifier _runtimeMinifier;

        public MinifyJavaScriptResultAttribute()
        {
            _hostingEnvironment = Current.HostingEnvironment;
            _runtimeMinifier = Current.RuntimeMinifier;
        }

        public MinifyJavaScriptResultAttribute(IHostingEnvironment hostingEnvironment, IRuntimeMinifier runtimeMinifier)
        {
            _hostingEnvironment = hostingEnvironment;
            _runtimeMinifier = runtimeMinifier;
        }

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
            if (_hostingEnvironment.IsDebugMode) return;

            //minify the result
            var result = jsResult.Script;
            var minified = _runtimeMinifier.MinifyAsync(result, AssetType.Javascript).GetAwaiter().GetResult();
            jsResult.Script = minified;
        }
    }
}
