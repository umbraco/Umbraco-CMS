using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Packaging;

internal sealed class ImportPackageBuilder : ExpressionBuilderBase<ImportPackageBuilderExpression>, IImportPackageBuilder,
    IExecutableBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Packaging.ImportPackageBuilder"/> class.
    /// </summary>
    /// <param name="packagingService">An instance of <see cref="IPackagingService"/> used for package operations.</param>
    /// <param name="mediaService">An instance of <see cref="IMediaService"/> for managing media items.</param>
    /// <param name="mediaFileManager">The <see cref="MediaFileManager"/> responsible for handling media files.</param>
    /// <param name="mediaUrlGenerators">A collection of <see cref="MediaUrlGenerator"/> used to generate media URLs.</param>
    /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> used for string manipulation and formatting.</param>
    /// <param name="contentTypeBaseServiceProvider">The <see cref="IContentTypeBaseServiceProvider"/> for accessing content type services.</param>
    /// <param name="context">The <see cref="IMigrationContext"/> providing context for the migration process.</param>
    /// <param name="options">The <see cref="IOptions{PackageMigrationSettings}"/> containing package migration settings.</param>
    public ImportPackageBuilder(
        IPackagingService packagingService,
        IMediaService mediaService,
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IShortStringHelper shortStringHelper,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        IMigrationContext context,
        IOptions<PackageMigrationSettings> options)
        : base(new ImportPackageBuilderExpression(
            packagingService,
            mediaService,
            mediaFileManager,
            mediaUrlGenerators,
            shortStringHelper,
            contentTypeBaseServiceProvider,
            context,
            options))
    {
    }

    /// <summary>
    /// Executes the import operation for the package represented by this builder.
    /// </summary>
    public void Do() => Expression.Execute();

    /// <summary>
    /// Specifies that the package migration should be loaded from an embedded resource of the given migration type.
    /// </summary>
    /// <typeparam name="TPackageMigration">The type of the migration class that derives from <see cref="AsyncPackageMigrationBase"/> and represents the embedded resource migration to use.</typeparam>
    /// <returns>The current <see cref="IExecutableBuilder"/> instance for method chaining.</returns>
    public IExecutableBuilder FromEmbeddedResource<TPackageMigration>()
        where TPackageMigration : AsyncPackageMigrationBase
    {
        Expression.EmbeddedResourceMigrationType = typeof(TPackageMigration);
        return this;
    }

    /// <summary>
    /// Configures the builder to use a package migration defined as an embedded resource, specified by the given migration type.
    /// </summary>
    /// <param name="packageMigrationType">The <see cref="Type"/> representing a class that inherits from <see cref="AsyncPackageMigrationBase"/> and defines the migration logic to be executed from an embedded resource.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> instance for further configuration or execution.</returns>
    public IExecutableBuilder FromEmbeddedResource(Type packageMigrationType)
    {
        Expression.EmbeddedResourceMigrationType = packageMigrationType;
        return this;
    }

    /// <summary>
    /// Sets the package data manifest using the specified XML document.
    /// </summary>
    /// <param name="packageDataManifest">An <see cref="XDocument"/> representing the package data manifest.</param>
    /// <returns>This <see cref="ImportPackageBuilder"/> instance for method chaining.</returns>
    public IExecutableBuilder FromXmlDataManifest(XDocument packageDataManifest)
    {
        Expression.PackageDataManifest = packageDataManifest;
        return this;
    }
}
