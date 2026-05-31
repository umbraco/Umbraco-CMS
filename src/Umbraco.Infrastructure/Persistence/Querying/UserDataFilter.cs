namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
/// Provides filtering criteria for querying user-related data in the persistence layer.
/// </summary>
public class UserDataFilter : IUserDataFilter
{
    /// <summary>
    /// Gets or sets the collection of user keys to filter by.
    /// </summary>
    public ICollection<Guid>? UserKeys { get; set; }

    /// <summary>
    /// Gets or sets the collection of user group names used to filter user data.
    /// </summary>
    public ICollection<string>? Groups { get; set; }

    /// <summary>
    /// Gets or sets the collection of identifiers used to filter user data.
    /// </summary>
    public ICollection<string>? Identifiers { get; set; }
}
