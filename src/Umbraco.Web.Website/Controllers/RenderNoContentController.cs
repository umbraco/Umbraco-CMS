using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers
{
    public class RenderNoContentController : Controller
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly GlobalSettings _globalSettings;

        public RenderNoContentController(IUmbracoContextAccessor umbracoContextAccessor, IIOHelper ioHelper, IOptionsSnapshot<GlobalSettings> globalSettings)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        }

        public ActionResult Index()
        {
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var store = umbracoContext.Content;
            if (store?.HasContent() ?? false)
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
