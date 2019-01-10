using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Creates packages
    /// </summary>
    public interface IPackageBuilder
    {
        IEnumerable<PackageDefinition> GetAll();
        PackageDefinition GetById(int id);
        void Delete(int id);

        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns>
        /// true if creating/updating the package was successful, otherwise false
        /// </returns>
        bool SavePackage(PackageDefinition definition);

        /// <summary>
        /// Creates the package file and returns it's physical path
        /// </summary>
        /// <param name="definition"></param>
        string ExportPackage(PackageDefinition definition);
    }
}
