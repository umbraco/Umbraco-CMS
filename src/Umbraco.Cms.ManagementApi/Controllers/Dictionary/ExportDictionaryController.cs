using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class ExportDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IEntityXmlSerializer _entityXmlSerializer;

    public ExportDictionaryController(ILocalizationService localizationService, IEntityXmlSerializer entityXmlSerializer)
    {
        _localizationService = localizationService;
        _entityXmlSerializer = entityXmlSerializer;
    }

    [HttpGet("export/{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportDictionary(Guid key, bool includeChildren = false)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(key);
        if (dictionaryItem is null)
        {
            return await Task.FromResult(NotFound("No dictionary item found with id "));
        }

        XElement xml = _entityXmlSerializer.Serialize(dictionaryItem, includeChildren);

        var fileName = $"{dictionaryItem.ItemKey}.udt";

        // Set custom header so umbRequestHelper.downloadFile can save the correct filename
        HttpContext.Response.Headers.Add("x-filename", fileName);

        return await Task.FromResult(File(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet, fileName));
    }
}
