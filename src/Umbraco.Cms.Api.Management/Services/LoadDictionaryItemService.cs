using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;

namespace Umbraco.Cms.Api.Management.Services;

public class LoadDictionaryItemService : ILoadDictionaryItemService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    // FIXME: use IDictionaryItemService instead of ILocalizationService
    private readonly ILocalizationService _localizationService;
    private readonly PackageDataInstallation _packageDataInstallation;
    private readonly ILogger<LoadDictionaryItemService> _logger;

    public LoadDictionaryItemService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizationService localizationService,
        PackageDataInstallation packageDataInstallation,
        ILogger<LoadDictionaryItemService> logger)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _localizationService = localizationService;
        _packageDataInstallation = packageDataInstallation;
        _logger = logger;
    }

    // FIXME: use Guid key, not integer ID for parent identification
    public IDictionaryItem Load(string filePath, int? parentId)
    {
        var xmlDocument = new XmlDocument { XmlResolver = null };
        xmlDocument.Load(filePath);

        var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? 0;
        var element = XElement.Parse(xmlDocument.InnerXml);

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

        return dictionaryItems.First();
    }
}
