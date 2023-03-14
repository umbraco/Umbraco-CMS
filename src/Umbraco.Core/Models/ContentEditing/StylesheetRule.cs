using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "stylesheetRule", Namespace = "")]
public class StylesheetRule
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = null!;

    [DataMember(Name = "selector")]
    public string Selector { get; set; } = null!;

    [DataMember(Name = "styles")]
    public string Styles { get; set; } = null!;
}
