using Microsoft.AspNet.Identity;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Ensure that both the normal password validator rules are processed along with the underlying membership provider rules
    /// </summary>
    public class ConfiguredPasswordValidator : PasswordValidator
    {
        public ConfiguredPasswordValidator(IPasswordConfiguration config)
        {
            RequiredLength = config.RequiredLength;
            RequireNonLetterOrDigit = config.RequireNonLetterOrDigit;
            RequireDigit = config.RequireDigit;
            RequireLowercase = config.RequireLowercase;
            RequireUppercase = config.RequireUppercase;
        }
    }
}
