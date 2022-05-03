using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers;

public class RenderNoContentController : Controller
{
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    [Obsolete("Please use constructor that takes an IHostingEnvironment instead")]
    public RenderNoContentController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IIOHelper ioHelper,
        IOptionsSnapshot<GlobalSettings> globalSettings)
    : this(umbracoContextAccessor, globalSettings, StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public RenderNoContentController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        _hostingEnvironment = hostingEnvironment;
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

        var model = new NoNodesViewModel { UmbracoPath = _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoPath) };

        return View(_globalSettings.NoNodesViewPath, model);
    }
}
