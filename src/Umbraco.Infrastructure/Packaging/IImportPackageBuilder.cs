using System.Xml.Linq;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <summary>
/// Represents a contract for building and importing packages into Umbraco CMS.
/// </summary>
public interface IImportPackageBuilder : IFluentBuilder
{
    /// <summary>
    /// Initializes the import package builder using an embedded resource associated with the specified <typeparamref name="TPackageMigration"/>.
    /// </summary>
    /// <typeparam name="TPackageMigration">The type of the package migration, which must inherit from <see cref="AsyncPackageMigrationBase"/>.</typeparam>
    /// <returns>An <see cref="IExecutableBuilder"/> for executing the package migration.</returns>
    IExecutableBuilder FromEmbeddedResource<TPackageMigration>()
        where TPackageMigration : AsyncPackageMigrationBase;

    /// <summary>
    /// Configures the import process to use a package migration defined in an embedded resource.
    /// </summary>
    /// <param name="packageMigrationType">The <see cref="Type"/> representing the package migration to import from the embedded resource.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> instance for executing the import process.</returns>
    IExecutableBuilder FromEmbeddedResource(Type packageMigrationType);

    /// <summary>
    /// Initializes the import package builder using the specified XML data manifest.
    /// </summary>
    /// <param name="packageDataManifest">An <see cref="XDocument"/> containing the package data manifest.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> instance to continue the import process.</returns>
    IExecutableBuilder FromXmlDataManifest(XDocument packageDataManifest);
}
