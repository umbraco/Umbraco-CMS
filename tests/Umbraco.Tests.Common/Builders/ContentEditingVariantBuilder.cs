using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
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

    public IReadOnlyCollection<ContentEditingPropertyValueBuilder<ContentEditingVariantBuilder<TParent>>> GetProperties()
    {
        if (_culture is null)
        {
            throw new InvalidOperationException("Culture must be defined for the variant before building.");
        }

        return _properties.Select(property => property.WithCulture(_culture)).ToList();
    }

    public override VariantModel Build() =>
        new()
        {
            Culture = _culture,
            Segment = _segment,
            Name = _name
        };
}
