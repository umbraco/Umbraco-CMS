using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
public class MoveDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IDictionaryService _dictionaryService;

    public MoveDictionaryController(
        ILocalizationService localizationService,
        ILocalizedTextService localizedTextService,
        IUmbracoMapper umbracoMapper,
        IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _localizedTextService = localizedTextService;
        _dictionaryService = dictionaryService;
    }

    /// <summary>
    ///     Changes the structure for dictionary items
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Move(MoveOrCopy move)
    {
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(move.Id);
        if (dictionaryItem == null)
        {
            return ValidationProblem(_localizedTextService.Localize("dictionary", "itemDoesNotExists"));
        }

        IDictionaryItem? parent = _localizationService.GetDictionaryItemById(move.ParentId);
        if (parent == null)
        {
            if (move.ParentId == Constants.System.Root)
            {
                dictionaryItem.ParentId = null;
            }
            else
            {
                return ValidationProblem(_localizedTextService.Localize("dictionary", "parentDoesNotExists"));
            }
        }
        else
        {
            dictionaryItem.ParentId = parent.Key;
            if (dictionaryItem.Key == parent.ParentId)
            {
                return ValidationProblem(_localizedTextService.Localize("moveOrCopy", "notAllowedByPath"));
            }
        }

        _localizationService.Save(dictionaryItem);

        var path = _dictionaryService.CalculatePath(dictionaryItem.ParentId, dictionaryItem.Id);

        return Content(path, MediaTypeNames.Text.Plain, Encoding.UTF8);
    }
}
