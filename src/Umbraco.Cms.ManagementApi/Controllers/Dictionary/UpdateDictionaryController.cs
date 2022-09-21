using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Json.Patch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Serialization;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDictionaryService _dictionaryService;
    private readonly IDictionaryFactory _dictionaryFactory;
    private readonly IJsonPatchService _jsonPatchService;
    private readonly ISystemTextJsonSerializer _systemTextJsonSerializer;

    public UpdateDictionaryController(
        ILocalizationService localizationService,
        IUmbracoMapper umbracoMapper,
        IDictionaryService dictionaryService,
        IDictionaryFactory dictionaryFactory,
        IJsonPatchService jsonPatchService,
        ISystemTextJsonSerializer systemTextJsonSerializer)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
        _dictionaryService = dictionaryService;
        _dictionaryFactory = dictionaryFactory;
        _jsonPatchService = jsonPatchService;
        _systemTextJsonSerializer = systemTextJsonSerializer;
    }

    [HttpPatch("{id:Guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, JsonPatchViewModel[] updateViewModel)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(id);

        if (dictionaryItem is null)
        {
            return NotFound();
        }

        DictionaryViewModel dictionaryToPatch = _umbracoMapper.Map<DictionaryViewModel>(dictionaryItem)!;

        PatchResult? result = _jsonPatchService.Patch(updateViewModel, dictionaryToPatch);

        if (result?.Result is null)
        {
            throw new JsonException("Could not patch the JsonPatchViewModel");
        }

        DictionaryViewModel? updatedDictionaryItem = _systemTextJsonSerializer.Deserialize<DictionaryViewModel>(result.Result.ToJsonString());
        if (updatedDictionaryItem is null)
        {
            throw new JsonException("Could not serialize from PatchResult to DictionaryViewModel");
        }

        IDictionaryItem dictionaryToSave = _dictionaryFactory.CreateDictionaryItem(updatedDictionaryItem!);
        _localizationService.Save(dictionaryToSave);
        return await Task.FromResult(Content(_dictionaryService.CalculatePath(dictionaryToSave.ParentId, dictionaryToSave.Id), MediaTypeNames.Text.Plain, Encoding.UTF8));
    }
}
