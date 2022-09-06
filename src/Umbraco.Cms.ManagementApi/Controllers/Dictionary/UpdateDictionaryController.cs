using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
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

    public UpdateDictionaryController(
        ILocalizationService localizationService,
        IUmbracoMapper umbracoMapper,
        IDictionaryService dictionaryService,
        IDictionaryFactory dictionaryFactory)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
        _dictionaryService = dictionaryService;
        _dictionaryFactory = dictionaryFactory;
    }

    [HttpPatch("update/{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, JsonPatchViewModel[] updateViewModel)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(id);
        DictionaryViewModel dictionaryToPatch = _umbracoMapper.Map<DictionaryViewModel>(dictionaryItem)!;
        var patchString = JsonSerializer.Serialize(updateViewModel);
        var docString = JsonSerializer.Serialize(dictionaryToPatch);
        JsonPatch? patch = JsonSerializer.Deserialize<JsonPatch>(patchString);
        var doc = JsonNode.Parse(docString);
        PatchResult? result = patch?.Apply(doc);
        DictionaryViewModel? updatedDictionaryItem = JsonSerializer.Deserialize<DictionaryViewModel>(result?.Result);
        IDictionaryItem dictionaryToSave = _dictionaryFactory.CreateDictionary(updatedDictionaryItem!);
        _localizationService.Save(dictionaryToSave);
        return Content(_dictionaryService.CalculatePath(dictionaryToSave.ParentId, dictionaryToSave.Id), MediaTypeNames.Text.Plain, Encoding.UTF8);
    }
}
