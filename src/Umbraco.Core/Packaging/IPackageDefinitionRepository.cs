namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Defines methods for persisting package definitions to storage
/// </summary>
public interface IPackageDefinitionRepository
{
    IEnumerable<PackageDefinition?> GetAll();

    PackageDefinition? GetById(int id);

    void Delete(int id);

    /// <summary>
    ///     Persists a package definition to storage
    /// </summary>
    /// <returns>
    ///     true if creating/updating the package was successful, otherwise false
    /// </returns>
    bool SavePackage(PackageDefinition definition);
}
