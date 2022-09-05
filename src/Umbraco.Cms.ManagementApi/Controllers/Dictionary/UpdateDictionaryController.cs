using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
public class UpdateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDictionaryService _dictionaryService;

    public UpdateDictionaryController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper, IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
        _dictionaryService = dictionaryService;
    }

    [HttpPatch("update/{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody]JsonPatchDocument<DictionaryViewModel> patchDoc)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(id);
        DictionaryViewModel dictionaryToPatch = _umbracoMapper.Map<DictionaryViewModel>(dictionaryItem)!;
        patchDoc.ApplyTo(dictionaryToPatch);
        IDictionaryItem dictionaryToSave = _umbracoMapper.Map<IDictionaryItem>(dictionaryToPatch)!;
        _localizationService.Save(dictionaryToSave);
        return Content(_dictionaryService.CalculatePath(dictionaryToSave.ParentId, dictionaryToSave.Id), MediaTypeNames.Text.Plain, Encoding.UTF8);
    }
}
