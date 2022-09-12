﻿using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.ViewModels.Culture;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Culture;

[ApiVersion("1.0")]
public class GetAllCultureController : CultureControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;

    public GetAllCultureController(IUmbracoMapper umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
    }
    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IDictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<CultureViewModel>> GetAll(int skip, int take)
    {
        IEnumerable<CultureInfo> list = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .DistinctBy(x => x.Name)
            .OrderBy(x => x.EnglishName)
            .Skip(skip)
            .Take(take);

        return _umbracoMapper.Map<PagedViewModel<CultureViewModel>>(list)!;
    }
}
