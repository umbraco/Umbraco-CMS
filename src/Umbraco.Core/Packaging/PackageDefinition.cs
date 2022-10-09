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
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "packageGuid")]
    public Guid PackageId { get; set; }

    [DataMember(Name = "name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The full path to the package's XML file.
    /// </summary>
    [ReadOnly(true)]
    [DataMember(Name = "packagePath")]
    public string PackagePath { get; set; } = string.Empty;

    [DataMember(Name = "contentLoadChildNodes")]
    public bool ContentLoadChildNodes { get; set; }

    [DataMember(Name = "contentNodeId")]
    public string? ContentNodeId { get; set; }

    [DataMember(Name = "macros")]
    public IList<string> Macros { get; set; } = new List<string>();

    [DataMember(Name = "languages")]
    public IList<string> Languages { get; set; } = new List<string>();

    [DataMember(Name = "dictionaryItems")]
    public IList<string> DictionaryItems { get; set; } = new List<string>();

    [DataMember(Name = "templates")]
    public IList<string> Templates { get; set; } = new List<string>();

    [DataMember(Name = "partialViews")]
    public IList<string> PartialViews { get; set; } = new List<string>();

    [DataMember(Name = "documentTypes")]
    public IList<string> DocumentTypes { get; set; } = new List<string>();

    [DataMember(Name = "mediaTypes")]
    public IList<string> MediaTypes { get; set; } = new List<string>();

    [DataMember(Name = "stylesheets")]
    public IList<string> Stylesheets { get; set; } = new List<string>();

    [DataMember(Name = "scripts")]
    public IList<string> Scripts { get; set; } = new List<string>();

    [DataMember(Name = "dataTypes")]
    public IList<string> DataTypes { get; set; } = new List<string>();

    [DataMember(Name = "mediaUdis")]
    public IList<GuidUdi> MediaUdis { get; set; } = new List<GuidUdi>();

    [DataMember(Name = "mediaLoadChildNodes")]
    public bool MediaLoadChildNodes { get; set; }
}
