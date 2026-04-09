using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
public class IsoCodeValidator : IIsoCodeValidator
{
    /// <inheritdoc />
    public bool IsValid(CultureInfo culture) => string.IsNullOrEmpty(culture.Name) is false
        && culture.CultureTypes.HasFlag(CultureTypes.UserCustomCulture) is false;
}
