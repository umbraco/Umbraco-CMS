using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     An object representing the property type validation settings
/// </summary>
[DataContract(Name = "propertyValidation", Namespace = "")]
public class PropertyTypeValidation
{
    [DataMember(Name = "mandatory")]
    public bool Mandatory { get; set; }

    [DataMember(Name = "mandatoryMessage")]
    public string? MandatoryMessage { get; set; }

    [DataMember(Name = "pattern")]
    public string? Pattern { get; set; }

    [DataMember(Name = "patternMessage")]
    public string? PatternMessage { get; set; }
}
