namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Helper for validating the number of items held by a property editor against a configured minimum.
/// </summary>
public static class ItemCountValidationHelper
{
    /// <summary>
    ///     Determines whether a count is above zero and below the configured minimum.
    /// </summary>
    /// <param name="count">The number of items held.</param>
    /// <param name="minimum">The configured minimum, where <c>null</c> or zero means no minimum.</param>
    /// <returns><c>true</c> when the count is above zero and below the minimum; otherwise <c>false</c>.</returns>
    public static bool IsAboveZeroAndBelowMinimum(int count, int? minimum)
        => minimum > 0 && count > 0 && count < minimum;
}
