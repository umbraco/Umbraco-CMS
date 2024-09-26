using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeContainerBuilder<TParent, TModel>(TParent parentBuilder)
    : ChildBuilderBase<TParent, TModel>(parentBuilder)
    where TModel : PropertyTypeContainerModelBase, new()
{
    private readonly TModel _model = new();
    private Guid? _key;
    private Guid? _parentKey;
    private string _name;
    private string _type;
    private int? _sortOrder;

    public PropertyTypeContainerBuilder<TParent, TModel> WithKey(Guid key)
    {
        _key = key;
        return this;
    }

    public PropertyTypeContainerBuilder<TParent, TModel> WithParentKey(Guid parentKey)
    {
        _parentKey = parentKey;
        return this;
    }

    public PropertyTypeContainerBuilder<TParent, TModel> WithName(string name)
    {
        _name = name;
        return this;
    }

    public PropertyTypeContainerBuilder<TParent, TModel> WithType(string type)
    {
        _type = type;
        return this;
    }

    public PropertyTypeContainerBuilder<TParent, TModel> WithSortOrder(int sortOrder)
    {
        _model.SortOrder = sortOrder;
        return this;
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
