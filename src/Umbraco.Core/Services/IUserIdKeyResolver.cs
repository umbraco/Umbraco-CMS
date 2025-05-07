namespace Umbraco.Cms.Core.Services;

public interface IUserIdKeyResolver
{
    /// <summary>
    /// Tries to resolve a user key to a user id without fetching the entire user.
    /// </summary>
    /// <param name="key">The key of the user.</param>
    /// <returns>The id of the user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no user was found with the specified key.</exception>
    public Task<int> GetAsync(Guid key);

    /// <summary>
    /// Tries to resolve a user id to a user key without fetching the entire user.
    /// </summary>
    /// <param name="id">The id of the user.</param>
    /// <returns>The key of the user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no user was found with the specified id.</exception>
    public Task<Guid> GetAsync(int id);
}
