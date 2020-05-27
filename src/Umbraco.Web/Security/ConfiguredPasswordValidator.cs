using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    // NOTE: Migrated to netcore (in a different way)
    public interface IPasswordValidator
    {
        Task<Attempt<IEnumerable<string>>> ValidateAsync(IPasswordConfiguration config, string password);
    }

    // NOTE: Migrated to netcore (in a different way)
    public class ConfiguredPasswordValidator : PasswordValidator, IPasswordValidator
    {
        async Task<Attempt<IEnumerable<string>>> IPasswordValidator.ValidateAsync(IPasswordConfiguration passwordConfiguration, string password)
        {
            RequiredLength = passwordConfiguration.RequiredLength;
            RequireNonLetterOrDigit = passwordConfiguration.RequireNonLetterOrDigit;
            RequireDigit = passwordConfiguration.RequireDigit;
            RequireLowercase = passwordConfiguration.RequireLowercase;
            RequireUppercase = passwordConfiguration.RequireUppercase;

            var result = await ValidateAsync(password);
            if (result.Succeeded)
                return Attempt<IEnumerable<string>>.Succeed();
            return Attempt<IEnumerable<string>>.Fail(result.Errors);
        }
    }
}
