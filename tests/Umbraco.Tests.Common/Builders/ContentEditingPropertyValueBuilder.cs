using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingPropertyValueBuilder<TParent>(TParent parentBuilder)
    : ChildBuilderBase<TParent, PropertyValueModel>(parentBuilder), IWithAliasBuilder, IWithValueBuilder, IWithCultureBuilder, IWithSegmentBuilder
{
    private string _alias;
    private object? _value;
    private string? _culture;
    private string? _segment;

    string IWithAliasBuilder.Alias
    {
        get => _alias;
        set => _alias = value;
    }

    object? IWithValueBuilder.Value
    {
        get => _value;
        set => _value = value;
    }

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

    public override PropertyValueModel Build() => new() { Alias = _alias, Value = _value, Culture = _culture };
}
