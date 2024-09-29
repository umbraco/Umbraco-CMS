using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingVariantBuilder<TParent> : ChildBuilderBase<TParent, VariantModel>
{
    private string? _culture;
    private string? _segment;
    private string _name;

    private readonly List<ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>>>
        _properties = new();

    public ContentEditingVariantBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    public ContentEditingVariantBuilder<TParent> WithCulture(string culture)
    {
        _culture = culture;
        return this;
    }

    public ContentEditingVariantBuilder<TParent> WithSegment(string segment)
    {
        _segment = segment;
        return this;
    }

    public ContentEditingVariantBuilder<TParent> WithName(string name)
    {
        _name = name;
        return this;
    }

    public ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>> AddProperty()
    {
        var builder =
            new ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>>(
                (ContentEditingVariantBuilder<TParent>)this);
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
