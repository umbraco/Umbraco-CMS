using System.Xml.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Packaging;

/// <summary>
///     Compiled representation of a content base (Document or Media)
/// </summary>
public class CompiledPackageContentBase
{
    /// <summary>
    ///     Gets or sets the import mode for the content.
    /// </summary>
    public string? ImportMode { get; set; } // this is never used

    /// <summary>
    ///     The serialized version of the content
    /// </summary>
    public XElement XmlData { get; set; } = null!;

    /// <summary>
    ///     Creates a new <see cref="CompiledPackageContentBase" /> from an XML element.
    /// </summary>
    /// <param name="xml">The XML element containing the content data.</param>
    /// <returns>A new compiled package content base instance.</returns>
    public static CompiledPackageContentBase Create(XElement xml) =>
        new() { XmlData = xml, ImportMode = xml.AttributeValue<string>("importMode") };
}
