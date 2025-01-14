using System.Globalization;

namespace Umbraco.Cms.Core.Services;

public class CultureService : ICultureService
{
    private readonly IIsoCodeValidator _isoCodeValidator;

    public CultureService(IIsoCodeValidator isoCodeValidator) => _isoCodeValidator = isoCodeValidator;

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
