using System.Xml.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Packaging;

/// <summary>
///     Compiled representation of a content base (Document or Media)
/// </summary>
public class CompiledPackageContentBase
{
    public string? ImportMode { get; set; } // this is never used

    /// <summary>
    ///     The serialized version of the content
    /// </summary>
    public XElement XmlData { get; set; } = null!;

    public static CompiledPackageContentBase Create(XElement xml) =>
        new() { XmlData = xml, ImportMode = xml.AttributeValue<string>("importMode") };
}
