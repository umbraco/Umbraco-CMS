using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

/// <summary>
/// API controller responsible for exporting dictionary entries from the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ExportDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IEntityXmlSerializer _entityXmlSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportDictionaryController"/> class, providing services for dictionary item management and XML serialization.
    /// </summary>
    /// <param name="dictionaryItemService">Service used to manage dictionary items.</param>
    /// <param name="entityXmlSerializer">Service used to serialize entities to XML.</param>
    public ExportDictionaryController(IDictionaryItemService dictionaryItemService, IEntityXmlSerializer entityXmlSerializer)
    {
        _dictionaryItemService = dictionaryItemService;
        _entityXmlSerializer = entityXmlSerializer;
    }

    /// <summary>
    /// Exports the dictionary item identified by the provided <paramref name="id"/> as a downloadable file.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the dictionary item to export.</param>
    /// <param name="includeChildren">If <c>true</c>, child dictionary items will also be included in the export; otherwise, only the specified item is exported.</param>
    /// <returns>
    /// A <see cref="FileContentResult"/> containing the exported dictionary data as a file if the dictionary item is found;
    /// otherwise, a <see cref="NotFoundResult"/> if the item does not exist.
    /// </returns>
    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Exports a dictionary.")]
    [EndpointDescription("Exports the dictionary identified by the provided Id to a downloadable format.")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken, Guid id, bool includeChildren = false)
    {
        IDictionaryItem? dictionaryItem = await _dictionaryItemService.GetAsync(id);
        if (dictionaryItem is null)
        {
            return DictionaryItemNotFound();
        }

        XElement xml = _entityXmlSerializer.Serialize(dictionaryItem, includeChildren);

        return File(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet, $"{dictionaryItem.ItemKey}.udt");
    }
}
