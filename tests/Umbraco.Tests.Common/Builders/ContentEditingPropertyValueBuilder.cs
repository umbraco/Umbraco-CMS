using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentEditingPropertyValueBuilder<TParent> : ChildBuilderBase<TParent, PropertyValueModel>
{
    private string _alias;
    private object? _value;

    public ContentEditingPropertyValueBuilder(TParent parentBuilder)
        : base(parentBuilder)
    {
    }

    public ContentEditingPropertyValueBuilder<TParent> WithAlias(string alias)
    {
        _alias = alias;
        return this;
    }

    public ContentEditingPropertyValueBuilder<TParent> WithValue(object value)
    {
        _value = value;
        return this;
    }

    public override PropertyValueModel Build() => new() { Alias = _alias, Value = _value };
}
