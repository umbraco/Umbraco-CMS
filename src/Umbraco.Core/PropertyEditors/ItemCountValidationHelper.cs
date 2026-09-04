namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Helper for validating the number of items held by a property editor against a configured minimum.
/// </summary>
public static class ItemCountValidationHelper
{
    /// <summary>
    ///     Determines whether a collection holds fewer items than the configured minimum.
    /// </summary>
    /// <param name="count">The number of items held.</param>
    /// <param name="minimum">The configured minimum, where <c>null</c> or zero means no minimum.</param>
    /// <returns><c>true</c> when the minimum applies and is not met; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     A minimum applies only to a collection that is in use. Whether an empty collection is acceptable is decided
    ///     by the property's mandatory setting, via <see cref="IValueRequiredValidator" />. Configuring a data
    ///     type's minimum does not make an optional property required.
    /// </remarks>
    public static bool IsBelowMinimum(int count, int? minimum)
        => minimum > 0 && count > 0 && count < minimum;
}
