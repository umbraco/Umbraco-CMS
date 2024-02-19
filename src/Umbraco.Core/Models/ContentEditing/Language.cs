using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "language", Namespace = "")]
public class Language
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "culture", IsRequired = true)]
    [JsonPropertyName("culture")]
    [Required(AllowEmptyStrings = false)]
    public string IsoCode { get; set; } = null!;

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "isDefault")]
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [DataMember(Name = "isMandatory")]
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; }

    [DataMember(Name = "fallbackIsoCode")]
    [JsonPropertyName("fallbackIsoCode")]
    public string? FallbackIsoCode { get; set; }
}
