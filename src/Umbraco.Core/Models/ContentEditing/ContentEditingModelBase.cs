namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentEditingModelBase
{
    public IEnumerable<PropertyValueModel> Properties { get; set; } = Array.Empty<PropertyValueModel>();

    public IEnumerable<VariantModel> Variants { get; set; } = Array.Empty<VariantModel>();
}
