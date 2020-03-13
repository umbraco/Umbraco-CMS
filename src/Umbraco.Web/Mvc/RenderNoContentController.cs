using System;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    public class RenderNoContentController : Controller
    {
        private readonly IUmbracoContext _umbracoContext;
        private readonly IIOHelper _ioHelper;
        private readonly IGlobalSettings _globalSettings;

        public RenderNoContentController(IUmbracoContext umbracoContext, IIOHelper ioHelper, IGlobalSettings globalSettings)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
        }

        public ActionResult Index()
        {
            var store = _umbracoContext.Content;
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
