using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Web.Website.Controllers
{
    public class RenderNoContentController : Controller
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly IOptions<GlobalSettings> _globalSettings;

        public RenderNoContentController(IUmbracoContextAccessor umbracoContextAccessor, IIOHelper ioHelper, IOptions<GlobalSettings> globalSettings)
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
                UmbracoPath = _ioHelper.ResolveUrl(_globalSettings.Value.UmbracoPath),
            };

            return View(_globalSettings.Value.NoNodesViewPath, model);
        }
    }
}
