namespace Umbraco.Cms.Core.Security;

public interface IPasswordHasher
{
    /// <summary>
    ///     Hashes a password
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The password hashed.</returns>
    string HashPassword(string password);
}
