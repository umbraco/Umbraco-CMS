using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Parses the XML document contained in a compiled (zip) Umbraco package.
/// </summary>
public class CompiledPackageXmlParser
{
    private readonly ConflictingPackageData _conflictingPackageData;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompiledPackageXmlParser"/> class.
    /// </summary>
    /// <param name="conflictingPackageData">The service used to detect conflicting package data.</param>
    public CompiledPackageXmlParser(ConflictingPackageData conflictingPackageData) =>
        _conflictingPackageData = conflictingPackageData;

    /// <summary>
    ///     Converts an XML document to a <see cref="CompiledPackage"/> model.
    /// </summary>
    /// <param name="xml">The XML document representing the package.</param>
    /// <returns>A <see cref="CompiledPackage"/> instance containing the parsed package data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="xml"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the XML document is invalid.</exception>
    /// <exception cref="FormatException">Thrown when the XML document format is not a valid Umbraco package.</exception>
    public CompiledPackage ToCompiledPackage(XDocument xml)
    {
        if (xml is null)
        {
            throw new ArgumentNullException(nameof(xml));
        }

        if (xml.Root == null)
        {
            throw new InvalidOperationException("The xml document is invalid");
        }

        if (xml.Root.Name != "umbPackage")
        {
            throw new FormatException("The xml document is invalid");
        }

        XElement? info = xml.Root.Element("info");
        if (info == null)
        {
            throw new FormatException("The xml document is invalid");
        }

        XElement? package = info.Element("package");
        if (package == null)
        {
            throw new FormatException("The xml document is invalid");
        }

        var def = new CompiledPackage
        {
            // will be null because we don't know where this data is coming from and
            // this value is irrelevant during install.
            PackageFile = null,
            Name = package.Element("name")?.Value ?? string.Empty,
            PartialViews = xml.Root.Element("PartialViews")?.Elements("View") ?? Enumerable.Empty<XElement>(),
            Templates = xml.Root.Element("Templates")?.Elements("Template") ?? Enumerable.Empty<XElement>(),
            Stylesheets = xml.Root.Element("Stylesheets")?.Elements("Stylesheet") ?? Enumerable.Empty<XElement>(),
            Scripts = xml.Root.Element("Scripts")?.Elements("Script") ?? Enumerable.Empty<XElement>(),
            DataTypes = xml.Root.Element("DataTypes")?.Elements("DataType") ?? Enumerable.Empty<XElement>(),
            Languages = xml.Root.Element("Languages")?.Elements("Language") ?? Enumerable.Empty<XElement>(),
            DictionaryItems =
                xml.Root.Element("DictionaryItems")?.Elements("DictionaryItem") ?? Enumerable.Empty<XElement>(),
            DocumentTypes = xml.Root.Element("DocumentTypes")?.Elements("DocumentType") ?? Enumerable.Empty<XElement>(),
            MediaTypes = xml.Root.Element("MediaTypes")?.Elements("MediaType") ?? Enumerable.Empty<XElement>(),
            Documents =
                xml.Root.Element("Documents")?.Elements("DocumentSet")?.Select(CompiledPackageContentBase.Create) ??
                Enumerable.Empty<CompiledPackageContentBase>(),
            Media = xml.Root.Element("MediaItems")?.Elements()?.Select(CompiledPackageContentBase.Create) ??
                    Enumerable.Empty<CompiledPackageContentBase>(),
        };

        def.Warnings = GetInstallWarnings(def);

        return def;
    }

    /// <summary>
    ///     Gets the installation warnings for a compiled package by checking for conflicts.
    /// </summary>
    /// <param name="package">The compiled package to check for conflicts.</param>
    /// <returns>An <see cref="InstallWarnings"/> instance containing any detected conflicts.</returns>
    private InstallWarnings GetInstallWarnings(CompiledPackage package)
    {
        var installWarnings = new InstallWarnings
        {
            ConflictingTemplates = _conflictingPackageData.FindConflictingTemplates(package.Templates),
            ConflictingStylesheets = _conflictingPackageData.FindConflictingStylesheets(package.Stylesheets),
        };

        return installWarnings;
    }
}
