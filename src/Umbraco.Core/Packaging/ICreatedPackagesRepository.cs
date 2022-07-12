namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Manages the storage of created package definitions
/// </summary>
public interface ICreatedPackagesRepository : IPackageDefinitionRepository
{
    /// <summary>
    ///     Creates the package file and returns it's physical path
    /// </summary>
    /// <param name="definition"></param>
    string ExportPackage(PackageDefinition definition);
}
