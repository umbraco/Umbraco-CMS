using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions;

public static class PasswordConfigurationExtensions
{
    public static void ConfigurePasswordOptions(this PasswordOptions output, IPasswordConfiguration input)
    {
        output.RequiredLength = input.RequiredLength;
        output.RequireNonAlphanumeric = input.RequireNonLetterOrDigit;
        output.RequireDigit = input.RequireDigit;
        output.RequireLowercase = input.RequireLowercase;
        output.RequireUppercase = input.RequireUppercase;
    }
}
