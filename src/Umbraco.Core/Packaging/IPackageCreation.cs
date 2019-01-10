using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Creates packages
    /// </summary>
    public interface IPackageCreation
    {
        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns></returns>
        void SavePackageDefinition(PackageDefinition definition);

        /// <summary>
        /// Creates the package file and returns it's physical path
        /// </summary>
        /// <param name="definition"></param>
        string ExportPackageDefinition(PackageDefinition definition);
    }
}
