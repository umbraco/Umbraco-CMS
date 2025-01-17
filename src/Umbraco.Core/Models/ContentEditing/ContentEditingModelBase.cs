namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentEditingModelBase
{
    public string? InvariantName { get; set; }

    public IEnumerable<PropertyValueModel> InvariantProperties { get; set; } = Array.Empty<PropertyValueModel>();

    public IEnumerable<VariantModel> Variants { get; set; } = Array.Empty<VariantModel>();
}
