using System.Globalization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.ViewModels.Culture;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Culture;

[ApiVersion("1.0")]
public class AllCultureController : CultureControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ICultureService _cultureService;

    public AllCultureController(IUmbracoMapper umbracoMapper, ICultureService cultureService)
    {
        _umbracoMapper = umbracoMapper;
        _cultureService = cultureService;
    }

    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<CultureReponseModel>), StatusCodes.Status200OK)]
    public Task<PagedViewModel<CultureReponseModel>> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        CultureInfo[] all = _cultureService.GetValidCultureInfos();

        var viewModel = new PagedViewModel<CultureReponseModel>
        {
            Items = _umbracoMapper.MapEnumerable<CultureInfo, CultureReponseModel>(all.Skip(skip).Take(take)),
            Total = all.Length
        };

        return Task.FromResult(viewModel);
    }
}
