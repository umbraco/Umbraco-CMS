using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Generates a password
    /// </summary>
    public interface IPasswordGenerator
    {
        /// <summary>
        /// Generates a password
        /// </summary>
        /// <param name="passwordConfiguration"></param>
        /// <returns></returns>
        string GeneratePassword(IPasswordConfiguration passwordConfiguration);
    }
}
