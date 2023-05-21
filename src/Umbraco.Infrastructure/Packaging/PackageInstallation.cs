using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Packaging;

public class PackageInstallation : IPackageInstallation
{
    private readonly PackageDataInstallation _packageDataInstallation;
    private readonly CompiledPackageXmlParser _parser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageInstallation" /> class.
    /// </summary>
    public PackageInstallation(PackageDataInstallation packageDataInstallation, CompiledPackageXmlParser parser)
    {
        _packageDataInstallation =
            packageDataInstallation ?? throw new ArgumentNullException(nameof(packageDataInstallation));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public CompiledPackage ReadPackage(XDocument? packageXmlFile)
    {
        if (packageXmlFile == null)
        {
            throw new ArgumentNullException(nameof(packageXmlFile));
        }

        var compiledPackage = _parser.ToCompiledPackage(packageXmlFile);
        return compiledPackage;
    }

    public InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId, out PackageDefinition packageDefinition)
    {
        packageDefinition = new PackageDefinition { Name = compiledPackage.Name };

        InstallationSummary installationSummary = _packageDataInstallation.InstallPackageData(compiledPackage, userId);

        // Make sure the definition is up to date with everything (note: macro partial views are embedded in macros)
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

        foreach (IMacro x in installationSummary.MacrosInstalled)
        {
            packageDefinition.Macros.Add(x.Id.ToInvariantString());
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
