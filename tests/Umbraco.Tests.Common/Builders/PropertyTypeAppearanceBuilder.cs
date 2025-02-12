using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeAppearanceBuilder<TParent, TModel>
    : ChildBuilderBase<PropertyTypeEditingBuilder<TParent, TModel>, PropertyTypeAppearance>, IBuildPropertyTypes, IWithLabelOnTop
    where TModel : PropertyTypeModelBase, new()
{
    private bool? _labelOnTop;

    public PropertyTypeAppearanceBuilder(PropertyTypeEditingBuilder<TParent, TModel> parentBuilder)
        : base(parentBuilder)
    {
    }

    bool? IWithLabelOnTop.LabelOnTop
    {
        get => _labelOnTop;
        set => _labelOnTop = value;
    }

    public override PropertyTypeAppearance Build() => new() { LabelOnTop = _labelOnTop ?? false };
}
