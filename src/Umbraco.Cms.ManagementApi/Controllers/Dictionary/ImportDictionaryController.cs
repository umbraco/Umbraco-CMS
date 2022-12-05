using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class ImportDictionaryController : DictionaryControllerBase
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IDictionaryService _dictionaryService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILoadDictionaryItemService _loadDictionaryItemService;

    public ImportDictionaryController(
        IHostingEnvironment hostingEnvironment,
        IDictionaryService dictionaryService,
        IWebHostEnvironment webHostEnvironment,
        ILoadDictionaryItemService loadDictionaryItemService)
    {
        _hostingEnvironment = hostingEnvironment;
        _dictionaryService = dictionaryService;
        _webHostEnvironment = webHostEnvironment;
        _loadDictionaryItemService = loadDictionaryItemService;
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
        if (_webHostEnvironment.ContentRootFileProvider.GetFileInfo(filePath) is null)
        {
            return await Task.FromResult(NotFound());
        }

        IDictionaryItem dictionaryItem = _loadDictionaryItemService.Load(filePath, parentId);

        return await Task.FromResult(Content(_dictionaryService.CalculatePath(dictionaryItem.ParentId, dictionaryItem.Id), MediaTypeNames.Text.Plain, Encoding.UTF8));
    }
}
