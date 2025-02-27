using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A validator for validating if an ISO code string can be is valid.
/// </summary>
public interface IIsoCodeValidator
{
    /// <summary>
    /// Validates that a string is a valid ISO code.
    /// </summary>
    /// <param name="isoCode">The string to validate.</param>
    /// <returns>True if the string is a valid ISO code.</returns>
    bool IsValid(string isoCode)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(isoCode);
            return culture.Name.Equals(isoCode, StringComparison.InvariantCultureIgnoreCase) && IsValid(culture);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a cultureInfo is a valid culture info in Umbraco.
    /// </summary>
    /// <param name="culture">The culture info to validate.</param>
    /// <returns>True if the CultureInfo is valid in Umbraco.</returns>
    bool IsValid(CultureInfo culture);
}
