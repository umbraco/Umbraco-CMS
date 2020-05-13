using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.WebAssets;
using Umbraco.Net;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionResults;
using Umbraco.Web.WebAssets;

namespace Umbraco.Web.BackOffice.Controllers
{

    [Area(Umbraco.Core.Constants.Web.Mvc.BackOfficeArea)]
    public class BackOfficeController : Controller
    {
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BackOfficeController(IRuntimeMinifier runtimeMinifier, IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            _runtimeMinifier = runtimeMinifier;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Default()
        {
            return View();
        }

        /// <summary>
        /// Returns the JavaScript main file including all references found in manifests
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [HttpGet]
        public async Task<IActionResult> Application()
        {
            var result = await _runtimeMinifier.GetScriptForLoadingBackOfficeAsync(_globalSettings, _hostingEnvironment);

            return new JavaScriptResult(result);
        }
    }
}
