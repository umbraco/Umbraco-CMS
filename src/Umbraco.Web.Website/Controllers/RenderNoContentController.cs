using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Web.Website.Controllers;

public class RenderNoContentController : Controller
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly GlobalSettings _globalSettings;
    private readonly IDocumentUrlService _urlService;

    [ActivatorUtilitiesConstructor]
    public RenderNoContentController(
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IDocumentUrlService urlService)
    {
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        _urlService = urlService;
    }

    [Obsolete("Scheduled for removal in Umbraco 18")]
    public RenderNoContentController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IHostingEnvironment hostingEnvironment,
        IOptionsSnapshot<GlobalSettings> globalSettings)
    : this(hostingEnvironment, globalSettings, StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>())
    {
    }

    public ActionResult Index()
    {
        if (_urlService.HasAny())
        {
            // If there is actually content, go to the root.
            return Redirect("~/");
        }

        var model = new NoNodesViewModel { UmbracoPath = _hostingEnvironment.GetBackOfficePath() };

        return View(_globalSettings.NoNodesViewPath, model);
    }
}
