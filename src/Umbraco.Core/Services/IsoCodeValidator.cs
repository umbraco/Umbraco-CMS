using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class IsoCodeValidator : IIsoCodeValidator
{
    /// <inheritdoc />
    public bool IsValid(CultureInfo culture) => culture.CultureTypes.HasFlag(CultureTypes.UserCustomCulture) is false;
}
