using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for working with cultures and ISO codes.
/// </summary>
public class CultureService : ICultureService
{
    private readonly IIsoCodeValidator _isoCodeValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureService"/> class.
    /// </summary>
    /// <param name="isoCodeValidator">The ISO code validator.</param>
    public CultureService(IIsoCodeValidator isoCodeValidator) => _isoCodeValidator = isoCodeValidator;

    /// <inheritdoc />
    public CultureInfo[] GetValidCultureInfos()
    {
        CultureInfo[] all = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x=> _isoCodeValidator.IsValid(x))
            .DistinctBy(x => x.Name)
            .OrderBy(x => x.EnglishName)
            .ToArray();
        return all;
    }
}
