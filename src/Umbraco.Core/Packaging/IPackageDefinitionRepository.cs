namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Defines methods for persisting package definitions to storage.
/// </summary>
public interface IPackageDefinitionRepository
{
    /// <summary>
    ///     Gets all package definitions from storage.
    /// </summary>
    /// <returns>An enumerable collection of all <see cref="PackageDefinition"/> instances.</returns>
    IEnumerable<PackageDefinition?> GetAll();

    /// <summary>
    ///     Gets a package definition by its integer identifier.
    /// </summary>
    /// <param name="id">The identifier of the package definition.</param>
    /// <returns>The <see cref="PackageDefinition"/> if found; otherwise, <c>null</c>.</returns>
    PackageDefinition? GetById(int id);

    /// <summary>
    ///     Gets a package definition by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the package definition.</param>
    /// <returns>The <see cref="PackageDefinition"/> if found; otherwise, <c>null</c>.</returns>
    PackageDefinition? GetByKey(Guid key);

    /// <summary>
    ///     Deletes a package definition from storage.
    /// </summary>
    /// <param name="id">The identifier of the package definition to delete.</param>
    void Delete(int id);

    /// <summary>
    ///     Persists a package definition to storage.
    /// </summary>
    /// <param name="definition">The package definition to save.</param>
    /// <returns><c>true</c> if creating/updating the package was successful; otherwise, <c>false</c>.</returns>
    bool SavePackage(PackageDefinition definition);
}
