using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingVariantBuilder<TParent>(TParent parentBuilder)
    : ChildBuilderBase<TParent, VariantModel>(parentBuilder), IWithCultureBuilder, IWithSegmentBuilder, IWithNameBuilder
{
    private string? _culture;
    private string? _segment;
    private string _name;
    private readonly List<ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>>> _properties = new();

    string? IWithCultureBuilder.Culture
    {
        get => _culture;
        set => _culture = value;
    }

    string? IWithSegmentBuilder.Segment
    {
        get => _segment;
        set => _segment = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    public ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>> AddProperty()
    {
        var builder = new ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>>((ContentEditingVariantBuilder<TParent>)this);
        _properties.Add(builder);
        return builder;
    }

    public override VariantModel Build() =>
        new()
        {
            Culture = _culture,
            Segment = _segment,
            Name = _name,
            Properties = _properties.Select(x => x.Build()).ToList(),
        };
}
