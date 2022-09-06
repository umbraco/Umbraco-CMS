using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDictionaryService _dictionaryService;
    private readonly IDictionaryFactory _dictionaryFactory;
    private readonly IJsonPatchService _jsonPatchService;

    public UpdateDictionaryController(
        ILocalizationService localizationService,
        IUmbracoMapper umbracoMapper,
        IDictionaryService dictionaryService,
        IDictionaryFactory dictionaryFactory,
        IJsonPatchService jsonPatchService)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
        _dictionaryService = dictionaryService;
        _dictionaryFactory = dictionaryFactory;
        _jsonPatchService = jsonPatchService;
    }

    [HttpPatch("update/{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, JsonPatchViewModel[] updateViewModel)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(id);
        DictionaryViewModel dictionaryToPatch = _umbracoMapper.Map<DictionaryViewModel>(dictionaryItem)!;

        PatchResult? result = _jsonPatchService.Patch(updateViewModel, dictionaryToPatch);

        DictionaryViewModel? updatedDictionaryItem = JsonSerializer.Deserialize<DictionaryViewModel>(result?.Result);
        if (updatedDictionaryItem is null)
        {
            throw new JsonException("Could not serialize JsonNode to DictionaryViewModel");
        }

        IDictionaryItem dictionaryToSave = _dictionaryFactory.CreateDictionary(updatedDictionaryItem!);
        _localizationService.Save(dictionaryToSave);
        return Content(_dictionaryService.CalculatePath(dictionaryToSave.ParentId, dictionaryToSave.Id), MediaTypeNames.Text.Plain, Encoding.UTF8);
    }
}
