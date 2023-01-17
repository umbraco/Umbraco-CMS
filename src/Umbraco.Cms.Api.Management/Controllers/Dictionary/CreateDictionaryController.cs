using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using IDictionaryService = Umbraco.Cms.Api.Management.Services.Dictionary.IDictionaryService;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class CreateDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDictionaryFactory _dictionaryFactory;
    private readonly IDictionaryService _dictionaryService;

    public CreateDictionaryController(
        ILocalizationService localizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDictionaryFactory dictionaryFactory,
        IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dictionaryFactory = dictionaryFactory;
        _dictionaryService = dictionaryService;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> Create(DictionaryItemCreateModel dictionaryItemCreateModel)
    {
        ProblemDetails? collision = _dictionaryService.DetectNamingCollision(dictionaryItemCreateModel.Name);
        if (collision != null)
        {
            return Conflict(collision);
        }

        IDictionaryItem created = _dictionaryFactory.MapDictionaryItemCreate(dictionaryItemCreateModel);
        _localizationService.Save(created, CurrentUserId(_backOfficeSecurityAccessor));

        return await Task.FromResult(CreatedAtAction<ByKeyDictionaryController>(controller => nameof(controller.ByKey), created.Key));
    }
}
