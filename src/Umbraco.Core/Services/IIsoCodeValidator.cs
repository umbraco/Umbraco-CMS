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
    bool IsValid(string isoCode);
}
