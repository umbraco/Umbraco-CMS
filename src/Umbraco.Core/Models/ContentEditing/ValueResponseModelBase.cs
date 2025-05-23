using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ValueResponseModelBase : ValueModelBase
{
    [Required]
    public string EditorAlias { get; set; } = string.Empty;
}
