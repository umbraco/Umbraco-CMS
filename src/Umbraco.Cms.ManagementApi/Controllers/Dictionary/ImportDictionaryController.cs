using System.Net.Mime;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class ImportDictionaryController : DictionaryControllerBase
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly PackageDataInstallation _packageDataInstallation;
    private readonly ILogger<ImportDictionaryController> _logger;
    private readonly IDictionaryService _dictionaryService;

    public ImportDictionaryController(
        IHostingEnvironment hostingEnvironment,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizationService localizationService,
        PackageDataInstallation packageDataInstallation,
        ILogger<ImportDictionaryController> logger,
        IDictionaryService dictionaryService)
    {
        _hostingEnvironment = hostingEnvironment;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _localizationService = localizationService;
        _packageDataInstallation = packageDataInstallation;
        _logger = logger;
        _dictionaryService = dictionaryService;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportDictionary(string file, int? parentId)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return NotFound();
        }

        var filePath = Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data), file);
        if (!System.IO.File.Exists(filePath))
        {
            return await Task.FromResult(NotFound());
        }

        var xd = new XmlDocument { XmlResolver = null };
        xd.Load(filePath);

        var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? 0;
        var element = XElement.Parse(xd.InnerXml);

        IDictionaryItem? parentDictionaryItem = _localizationService.GetDictionaryItemById(parentId ?? 0);
        IEnumerable<IDictionaryItem> dictionaryItems = _packageDataInstallation.ImportDictionaryItem(element, userId, parentDictionaryItem?.Key);

        // Try to clean up the temporary file.
        try
        {
            System.IO.File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up temporary udt file in {File}", filePath);
        }

        IDictionaryItem dictionaryItem = dictionaryItems.First();

        return await Task.FromResult(Content(_dictionaryService.CalculatePath(dictionaryItem.ParentId, dictionaryItem.Id), MediaTypeNames.Text.Plain, Encoding.UTF8));
    }
}
