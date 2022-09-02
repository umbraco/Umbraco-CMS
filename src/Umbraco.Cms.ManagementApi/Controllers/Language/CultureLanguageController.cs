using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiVersion("1.0")]
public class CultureLanguageController : LanguageControllerBase
{
    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAllCultures")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IDictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IDictionary<string, string>> GetAllCultures()
        => CultureInfo.GetCultures(CultureTypes.AllCultures).DistinctBy(x => x.Name).OrderBy(x => x.EnglishName)
            .ToDictionary(x => x.Name, x => x.EnglishName);
}
