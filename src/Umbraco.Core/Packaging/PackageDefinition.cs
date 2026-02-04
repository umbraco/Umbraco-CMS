using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     A created package in the back office.
/// </summary>
/// <remarks>
///     This data structure is persisted to createdPackages.config when creating packages in the back office.
/// </remarks>
[DataContract(Name = "packageInstance")]
public class PackageDefinition
{
    /// <summary>
    ///     Gets or sets the unique identifier for this package definition.
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the globally unique identifier for this package.
    /// </summary>
    [DataMember(Name = "packageGuid")]
    public Guid PackageId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the package.
    /// </summary>
    [DataMember(Name = "name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The full path to the package's XML file.
    /// </summary>
    [ReadOnly(true)]
    [DataMember(Name = "packagePath")]
    public string PackagePath { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether child content nodes should be loaded.
    /// </summary>
    [DataMember(Name = "contentLoadChildNodes")]
    public bool ContentLoadChildNodes { get; set; }

    /// <summary>
    ///     Gets or sets the content node identifier to include in the package.
    /// </summary>
    [DataMember(Name = "contentNodeId")]
    public string? ContentNodeId { get; set; }

    /// <summary>
    ///     Gets or sets the collection of language identifiers included in this package.
    /// </summary>
    [DataMember(Name = "languages")]
    public IList<string> Languages { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of dictionary item identifiers included in this package.
    /// </summary>
    [DataMember(Name = "dictionaryItems")]
    public IList<string> DictionaryItems { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of template identifiers included in this package.
    /// </summary>
    [DataMember(Name = "templates")]
    public IList<string> Templates { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of partial view paths included in this package.
    /// </summary>
    [DataMember(Name = "partialViews")]
    public IList<string> PartialViews { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of document type identifiers included in this package.
    /// </summary>
    [DataMember(Name = "documentTypes")]
    public IList<string> DocumentTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of media type identifiers included in this package.
    /// </summary>
    [DataMember(Name = "mediaTypes")]
    public IList<string> MediaTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of stylesheet paths included in this package.
    /// </summary>
    [DataMember(Name = "stylesheets")]
    public IList<string> Stylesheets { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of script paths included in this package.
    /// </summary>
    [DataMember(Name = "scripts")]
    public IList<string> Scripts { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of data type identifiers included in this package.
    /// </summary>
    [DataMember(Name = "dataTypes")]
    public IList<string> DataTypes { get; set; } = new List<string>();

    /// <summary>
    ///     Gets or sets the collection of media UDIs included in this package.
    /// </summary>
    [DataMember(Name = "mediaUdis")]
    public IList<GuidUdi> MediaUdis { get; set; } = new List<GuidUdi>();

    /// <summary>
    ///     Gets or sets a value indicating whether child media nodes should be loaded.
    /// </summary>
    [DataMember(Name = "mediaLoadChildNodes")]
    public bool MediaLoadChildNodes { get; set; }
}
