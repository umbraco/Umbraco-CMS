using Microsoft.AspNetCore.Identity;

namespace Umbraco.Extensions;

public static class IdentityExtensions
{
    public static string ToErrorMessage(this IEnumerable<IdentityError> errors)
    {
        if (errors == null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        return string.Join(", ", errors.Select(x => x.Description).ToList());
    }
}
