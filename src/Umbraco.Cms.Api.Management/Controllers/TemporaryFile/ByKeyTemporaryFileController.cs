using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

public class ByKeyTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyTemporaryFileController(ITemporaryFileService temporaryFileService, IUmbracoMapper umbracoMapper)
    {
        _temporaryFileService = temporaryFileService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet($"{{{nameof(key)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LanguageViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel>> ByKey(Guid key)
    {
        TempFileModel? model = await _temporaryFileService.GetAsync(key);
        if (model == null)
        {
            return NotFound("The temporary file could not be found");
        }

        return Ok(_umbracoMapper.Map<TempFileModel, UploadSingleFileResponseModel>(model));
    }
}
