using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.UI.NetCore
{
    public class BergViewComponent : ViewComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BergViewComponent(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            TempData["BERG"] = "coool";

            var currentPage = _umbracoContextAccessor.UmbracoContext.PublishedRequest?.PublishedContent;
            return View(currentPage);
        }
    }
}
