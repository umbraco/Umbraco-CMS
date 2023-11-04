using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class GetListTranslationApiController : TranslationApiControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IRequestCultureService _requestCultureService;

    public GetListTranslationApiController(ILocalizationService localizationService, IRequestCultureService requestCultureService) : base(localizationService, requestCultureService)
    {
        _localizationService = localizationService;
        _requestCultureService = requestCultureService;
    }

    /// <summary>
    ///     Gets a json translation object based on your requested culture
    /// </summary>
    /// <param name="startItem">Optional start item guid</param>
    /// <returns>The translations.</returns>
    [HttpGet("list")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiTranslationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetList(Guid? startItem = null)
    {
        IDictionaryItem[] dictionaryItems = _localizationService.GetDictionaryItemDescendants(startItem).ToArray();

        object BuildJson(IDictionaryItem[] items, Guid? parentId)
        {
            var result = new Dictionary<string, object>();

            foreach (IDictionaryItem item in items.Where(t => t.ParentId == parentId))
            {
                var culture = _requestCultureService.GetRequestedCulture()?.ToLower();
                var translation = item.Translations.FirstOrDefault(x => x.LanguageIsoCode.ToLower() == culture);
                if (translation is not null)
                {
                    // Check if the item has children to determine if it should be a nested structure
                    IDictionaryItem[] children = items.Where(t => t.ParentId == item.Key).ToArray();
                    if (children.Any())
                    {
                        result[item.ItemKey] = BuildJson(items, item.Key);
                    }
                    else
                    {
                        result[item.ItemKey] = translation.Value;
                    }
                }
            }

            return result;
        }

        var jsonStructure = BuildJson(dictionaryItems, null);

        return await Task.FromResult(Json(jsonStructure));
    }


}
