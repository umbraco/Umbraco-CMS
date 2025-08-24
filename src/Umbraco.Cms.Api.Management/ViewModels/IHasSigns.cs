namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Marker interface that indicates the type has support for backoffice signs (presented as icons
/// overlaying the main icon for the entity).
/// </summary>
public interface IHasSigns
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the collection of signs for the entity.
    /// </summary>
    IEnumerable<SignModel> Signs { get; }

    /// <summary>
    /// Adds a sign to the entity with the specified alias.
    /// </summary>
    /// <param name="alias"></param>
    void AddSign(string alias);

    /// <summary>
    /// Removes a sign from the entity with the specified alias.
    /// </summary>
    /// <param name="alias"></param>
    void RemoveSign(string alias);
}
