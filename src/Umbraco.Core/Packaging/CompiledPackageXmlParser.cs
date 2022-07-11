using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Parses the xml document contained in a compiled (zip) Umbraco package
/// </summary>
public class CompiledPackageXmlParser
{
    private readonly ConflictingPackageData _conflictingPackageData;

    public CompiledPackageXmlParser(ConflictingPackageData conflictingPackageData) =>
        _conflictingPackageData = conflictingPackageData;

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
            Macros = xml.Root.Element("Macros")?.Elements("macro") ?? Enumerable.Empty<XElement>(),
            MacroPartialViews = xml.Root.Element("MacroPartialViews")?.Elements("View") ?? Enumerable.Empty<XElement>(),
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

    private InstallWarnings GetInstallWarnings(CompiledPackage package)
    {
        var installWarnings = new InstallWarnings
        {
            ConflictingMacros = _conflictingPackageData.FindConflictingMacros(package.Macros),
            ConflictingTemplates = _conflictingPackageData.FindConflictingTemplates(package.Templates),
            ConflictingStylesheets = _conflictingPackageData.FindConflictingStylesheets(package.Stylesheets),
        };

        return installWarnings;
    }
}
