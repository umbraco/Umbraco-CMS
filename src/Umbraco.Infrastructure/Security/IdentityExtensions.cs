using Microsoft.AspNetCore.Identity;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods that enhance identity-related operations.
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Converts a collection of <see cref="IdentityError"/> objects into a single error message string.
    /// </summary>
    /// <param name="errors">The collection of identity errors to convert. Cannot be <c>null</c>.</param>
    /// <returns>A single string containing all error descriptions, separated by commas.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <c>null</c>.</exception>
    public static string ToErrorMessage(this IEnumerable<IdentityError> errors)
    {
        if (errors == null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        return string.Join(", ", errors.Select(x => x.Description).ToList());
    }
}
