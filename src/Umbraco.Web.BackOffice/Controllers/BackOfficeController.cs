using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Runtime;
using Umbraco.Web.BackOffice.Filters;

namespace Umbraco.Web.BackOffice.Controllers
{
    public class BackOfficeController : Controller
    {
        private readonly IRuntimeMinifier _runtimeMinifier;

        public BackOfficeController(IRuntimeMinifier runtimeMinifier)
        {
            _runtimeMinifier = runtimeMinifier;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        public async Task<IActionResult> Application()
        {
            var result = await _runtimeMinifier.GetScriptForBackOfficeAsync();

            return new JavaScriptResult(result);
        }

        public IActionResult Reset()
        {
            _runtimeMinifier.Reset();

            return Content("OK");
        }
    }

    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}
