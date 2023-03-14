using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a macro parameter with an editor
/// </summary>
[DataContract(Name = "macroParameter", Namespace = "")]
public class MacroParameter
{
    [DataMember(Name = "alias", IsRequired = true)]
    [Required]
    public string Alias { get; set; } = null!;

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "sortOrder")]
    public int SortOrder { get; set; }

    /// <summary>
    ///     The editor view to render for this parameter
    /// </summary>
    [DataMember(Name = "view", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public string? View { get; set; }

    /// <summary>
    ///     The configuration for this parameter editor
    /// </summary>
    [DataMember(Name = "config", IsRequired = true)]
    [Required(AllowEmptyStrings = false)]
    public IDictionary<string, object>? Configuration { get; set; }

    /// <summary>
    ///     Since we don't post this back this isn't currently really used on the server side
    /// </summary>
    [DataMember(Name = "value")]
    public object? Value { get; set; }
}
