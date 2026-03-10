namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Represents a filter for querying user data.
/// </summary>
public interface IUserDataFilter
{
    /// <summary>
    ///     Gets or sets the collection of user keys to filter by.
    /// </summary>
    public ICollection<Guid>? UserKeys { get; set; }

    /// <summary>
    ///     Gets or sets the collection of group names to filter by.
    /// </summary>
    public ICollection<string>? Groups { get; set; }

    /// <summary>
    ///     Gets or sets the collection of identifiers to filter by.
    /// </summary>
    public ICollection<string>? Identifiers { get; set; }
}
