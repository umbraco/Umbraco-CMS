using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Runtime;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionResults;

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
    }
}
