using System;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    public class RenderNoContentController : Controller
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly GlobalSettings _globalSettings;

        public RenderNoContentController(IUmbracoContextAccessor umbracoContextAccessor, IIOHelper ioHelper, GlobalSettings globalSettings)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
        }

        public ActionResult Index()
        {
            var store = _umbracoContextAccessor.UmbracoContext.Content;
            if (store.HasContent())
            {
                // If there is actually content, go to the root.
                return Redirect("~/");
            }

            var model = new NoNodesViewModel
            {
                UmbracoPath = _ioHelper.ResolveUrl(_globalSettings.UmbracoPath),
            };

            return View(_globalSettings.NoNodesViewPath, model);
        }
    }
}
