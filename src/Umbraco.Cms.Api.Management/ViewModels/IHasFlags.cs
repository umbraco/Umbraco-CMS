namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Marker interface that indicates the type has support for backoffice flags (presented as icons
/// overlaying the main icon for the entity).
/// </summary>
public interface IHasFlags
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the collection of flag for the entity.
    /// </summary>
    IEnumerable<FlagModel> Flags { get; }

    /// <summary>
    /// Adds a flag to the entity with the specified alias.
    /// </summary>
    /// <param name="alias"></param>
    void AddFlag(string alias);

    /// <summary>
    /// Removes a flag from the entity with the specified alias.
    /// </summary>
    /// <param name="alias"></param>
    void RemoveFlag(string alias);
}
