using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeContainerBuilder<TParent, TModel>(TParent parentBuilder)
    : ChildBuilderBase<TParent, TModel>(parentBuilder), IWithKeyBuilder, IWithParentKeyBuilder, IWithNameBuilder, IWithTypeBuilder, IWithSortOrderBuilder where TModel : PropertyTypeContainerModelBase, new()
{
    private readonly TModel _model = new();
    private Guid? _key;
    private Guid? _parentKey;
    private string _name;
    private string _type;
    private int? _sortOrder;

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    Guid? IWithParentKeyBuilder.ParentKey
    {
        get => _parentKey;
        set => _parentKey = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    string IWithTypeBuilder.Type
    {
        get => _type;
        set => _type = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    public override TModel Build()
    {
        _model.Key = _key ?? Guid.NewGuid();

        if (_parentKey is not null)
        {
            _model.ParentKey = _parentKey;
        }

        _model.Name = _name ?? "Container";
        _model.Type = _type ?? "Group";
        _model.SortOrder = _sortOrder ?? 0;
        return _model;
    }
}
