using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging;

/// <summary>
/// Provides functionality for installing and managing Umbraco packages, including handling package files, dependencies, and installation steps.
/// </summary>
public class PackageInstallation : IPackageInstallation
{
    private readonly IPackageDataInstallation _packageDataInstallation;
    private readonly CompiledPackageXmlParser _parser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageInstallation" /> class.
    /// </summary>
    public PackageInstallation(IPackageDataInstallation packageDataInstallation, CompiledPackageXmlParser parser)
    {
        _packageDataInstallation =
            packageDataInstallation ?? throw new ArgumentNullException(nameof(packageDataInstallation));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    /// <summary>
    /// Reads the package information from the provided XML document.
    /// </summary>
    /// <param name="packageXmlFile">The XML document representing the package file. Cannot be <c>null</c>.</param>
    /// <returns>A <see cref="CompiledPackage"/> representing the compiled package data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageXmlFile"/> is <c>null</c>.</exception>
    public CompiledPackage ReadPackage(XDocument? packageXmlFile)
    {
        if (packageXmlFile == null)
        {
            throw new ArgumentNullException(nameof(packageXmlFile));
        }

        var compiledPackage = _parser.ToCompiledPackage(packageXmlFile);
        return compiledPackage;
    }

    /// <summary>
    /// Installs data from the specified compiled package, updating the package definition with details of the installed items.
    /// </summary>
    /// <param name="compiledPackage">The compiled package containing the data to be installed.</param>
    /// <param name="userId">The ID of the user performing the installation.</param>
    /// <param name="packageDefinition">When this method returns, contains a <see cref="PackageDefinition"/> populated with details of the installed items.</param>
    /// <returns>An <see cref="InstallationSummary"/> containing details of the installed package data.</returns>
    public InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId, out PackageDefinition packageDefinition)
    {
        packageDefinition = new PackageDefinition { Name = compiledPackage.Name };

        InstallationSummary installationSummary = _packageDataInstallation.InstallPackageData(compiledPackage, userId);

        // Make sure the definition is up to date with everything
        foreach (IDataType x in installationSummary.DataTypesInstalled)
        {
            packageDefinition.DataTypes.Add(x.Id.ToInvariantString());
        }

        foreach (ILanguage x in installationSummary.LanguagesInstalled)
        {
            packageDefinition.Languages.Add(x.Id.ToInvariantString());
        }

        foreach (IDictionaryItem x in installationSummary.DictionaryItemsInstalled)
        {
            packageDefinition.DictionaryItems.Add(x.Id.ToInvariantString());
        }

        foreach (ITemplate x in installationSummary.TemplatesInstalled)
        {
            packageDefinition.Templates.Add(x.Id.ToInvariantString());
        }

        foreach (IContentType x in installationSummary.DocumentTypesInstalled)
        {
            packageDefinition.DocumentTypes.Add(x.Id.ToInvariantString());
        }

        foreach (IMediaType x in installationSummary.MediaTypesInstalled)
        {
            packageDefinition.MediaTypes.Add(x.Id.ToInvariantString());
        }

        foreach (IFile x in installationSummary.StylesheetsInstalled)
        {
            packageDefinition.Stylesheets.Add(x.Path);
        }

        foreach (IScript x in installationSummary.ScriptsInstalled)
        {
            packageDefinition.Scripts.Add(x.Path);
        }

        foreach (IPartialView x in installationSummary.PartialViewsInstalled)
        {
            packageDefinition.PartialViews.Add(x.Path);
        }

        packageDefinition.ContentNodeId = installationSummary.ContentInstalled.FirstOrDefault()?.Id.ToInvariantString();

        foreach (IMedia x in installationSummary.MediaInstalled)
        {
            packageDefinition.MediaUdis.Add(x.GetUdi());
        }

        return installationSummary;
    }
}
