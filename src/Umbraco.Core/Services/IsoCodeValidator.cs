using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class IsoCodeValidator : IIsoCodeValidator
{
    private static readonly HashSet<string> _knownCultureNames = new(
        CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(culture => culture.Name),
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool IsValid(CultureInfo culture) =>

        // A culture is accepted when the platform recognises it. CultureTypes.UserCustomCulture cannot
        // establish that on its own: it records only the absence of a legacy Windows LCID, which many
        // recognised cultures lack. Such a culture is told apart from one the platform constructs on
        // demand for a well-formed but unassigned tag by whether it is among those enumerated.
        string.IsNullOrEmpty(culture.Name) is false
            && (culture.CultureTypes.HasFlag(CultureTypes.UserCustomCulture) is false
                || _knownCultureNames.Contains(culture.Name));
}
