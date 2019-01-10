using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    public interface ICreatedPackagesRepository : IPackageDefinitionRepository
    {
        /// <summary>
        /// Creates the package file and returns it's physical path
        /// </summary>
        /// <param name="definition"></param>
        string ExportPackage(PackageDefinition definition);
    }
}