using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class BackOfficeAssetsController : UmbracoAuthorizedJsonController
{
    private readonly IFileSystem _jsLibFileSystem;

    public BackOfficeAssetsController(IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory, IOptionsSnapshot<GlobalSettings> globalSettings)
    {
        var path = globalSettings.Value.UmbracoPath + Path.DirectorySeparatorChar + "lib";
        _jsLibFileSystem = new PhysicalFileSystem(
            ioHelper,
            hostingEnvironment,
            loggerFactory.CreateLogger<PhysicalFileSystem>(),
            hostingEnvironment.MapPathWebRoot(path),
            hostingEnvironment.ToAbsolute(path));
    }

    [HttpGet]
    public object GetSupportedLocales()
    {
        const string momentLocaleFolder = "moment";
        const string flatpickrLocaleFolder = "flatpickr/l10n";

        return new { moment = GetLocales(momentLocaleFolder), flatpickr = GetLocales(flatpickrLocaleFolder) };
    }

    private IEnumerable<string> GetLocales(string path)
    {
        var cultures = _jsLibFileSystem.GetFiles(path, "*.js").ToList();
        for (var i = 0; i < cultures.Count; i++)
        {
            cultures[i] = cultures[i].Substring(cultures[i].IndexOf(path, StringComparison.Ordinal) + path.Length + 1);
        }

        return cultures;
    }
}
