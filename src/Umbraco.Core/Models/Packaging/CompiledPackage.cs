using System.Xml.Linq;

namespace Umbraco.Cms.Core.Models.Packaging;

/// <summary>
///     The model of the umbraco package data manifest (xml file)
/// </summary>
public class CompiledPackage
{
    /// <summary>
    ///     Gets or sets the package file information.
    /// </summary>
    public FileInfo? PackageFile { get; set; }

    /// <summary>
    ///     Gets or sets the name of the package.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the installation warnings for the package.
    /// </summary>
    public InstallWarnings Warnings { get; set; } = new();

    /// <summary>
    ///     Gets or sets the collection of template XML elements.
    /// </summary>
    public IEnumerable<XElement> Templates { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of stylesheet XML elements.
    /// </summary>
    public IEnumerable<XElement> Stylesheets { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of script XML elements.
    /// </summary>
    public IEnumerable<XElement> Scripts { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of partial view XML elements.
    /// </summary>
    public IEnumerable<XElement> PartialViews { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of data type XML elements.
    /// </summary>
    public IEnumerable<XElement> DataTypes { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of language XML elements.
    /// </summary>
    public IEnumerable<XElement> Languages { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of dictionary item XML elements.
    /// </summary>
    public IEnumerable<XElement> DictionaryItems { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of document type XML elements.
    /// </summary>
    public IEnumerable<XElement> DocumentTypes { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of media type XML elements.
    /// </summary>
    public IEnumerable<XElement> MediaTypes { get; set; } = null!; // TODO: make strongly typed

    /// <summary>
    ///     Gets or sets the collection of compiled document content.
    /// </summary>
    public IEnumerable<CompiledPackageContentBase> Documents { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the collection of compiled media content.
    /// </summary>
    public IEnumerable<CompiledPackageContentBase> Media { get; set; } = null!;
}
