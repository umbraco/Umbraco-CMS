using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Culture;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Culture;

public class AllCultureController : CultureControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;

    public AllCultureController(IUmbracoMapper umbracoMapper) => _umbracoMapper = umbracoMapper;

    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<CultureViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<CultureViewModel>> GetAll(int skip, int take)
    {
        IEnumerable<CultureInfo> list = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .DistinctBy(x => x.Name)
            .OrderBy(x => x.EnglishName)
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(_umbracoMapper.Map<PagedViewModel<CultureViewModel>>(list)!);
    }
}
