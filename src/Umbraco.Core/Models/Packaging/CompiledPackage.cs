using System.Xml.Linq;

namespace Umbraco.Cms.Core.Models.Packaging;

/// <summary>
///     The model of the umbraco package data manifest (xml file)
/// </summary>
public class CompiledPackage
{
    public FileInfo? PackageFile { get; set; }

    public string Name { get; set; } = null!;

    public InstallWarnings Warnings { get; set; } = new();

    public IEnumerable<XElement> Macros { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> MacroPartialViews { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> Templates { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> Stylesheets { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> Scripts { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> PartialViews { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> DataTypes { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> Languages { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> DictionaryItems { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> DocumentTypes { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<XElement> MediaTypes { get; set; } = null!; // TODO: make strongly typed

    public IEnumerable<CompiledPackageContentBase> Documents { get; set; } = null!;

    public IEnumerable<CompiledPackageContentBase> Media { get; set; } = null!;
}
