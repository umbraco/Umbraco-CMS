using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class IsoCodeValidator : IIsoCodeValidator
{
    /// <inheritdoc />
    public bool IsValid(string isoCode)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(isoCode);
            return culture.Name.Equals(isoCode, StringComparison.InvariantCultureIgnoreCase) && culture.CultureTypes.HasFlag(CultureTypes.UserCustomCulture) == false;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
