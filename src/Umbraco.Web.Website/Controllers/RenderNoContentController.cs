using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers;

public class RenderNoContentController : Controller
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly GlobalSettings _globalSettings;

    public RenderNoContentController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<GlobalSettings> globalSettings)
    {
        _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
    }

    public ActionResult Index()
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        IPublishedContentCache? store = umbracoContext.Content;
        if (store?.HasContent() ?? false)
        {
            // If there is actually content, go to the root.
            return Redirect("~/");
        }

        var model = new NoNodesViewModel { UmbracoPath = _hostingEnvironment.GetBackOfficePath() };

        return View(_globalSettings.NoNodesViewPath, model);
    }
}
