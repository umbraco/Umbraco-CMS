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
    ///     Retrieve a paginated list of supported cultures for creating languages
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<CultureReponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Retrieve a paginated list of supported cultures for creating languages")]
    [EndpointDescription("Returns a paginated collection of supported culture information available in Umbraco. Use the `skip` and `take` parameters to control pagination. Each item includes culture-specific data such as name, display name, and locale identifiers.")]
    public async Task<PagedViewModel<CultureReponseModel>> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        CultureInfo[] all = _cultureService.GetValidCultureInfos();

        var viewModel = new PagedViewModel<CultureReponseModel>
        {
            Items = _umbracoMapper.MapEnumerable<CultureInfo, CultureReponseModel>(all.Skip(skip).Take(take)),
            Total = all.Length
        };

        return await Task.FromResult(viewModel);
    }
}
