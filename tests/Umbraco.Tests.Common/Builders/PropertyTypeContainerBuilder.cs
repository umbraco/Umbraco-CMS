using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyTypeContainerBuilder<TParent>(TParent parentBuilder)
    : ChildBuilderBase<TParent, ContentTypePropertyContainerModel>(parentBuilder),
        IBuildPropertyTypes, IWithKeyBuilder, IWithParentKeyBuilder, IWithNameBuilder, IWithTypeBuilder,
        IWithSortOrderBuilder
{
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

    public override ContentTypePropertyContainerModel Build()
    {
        var key = _key ?? Guid.NewGuid();
        var parentKey = _parentKey;
        var name = _name ?? "Container";
        var type = _type ?? "group";
        var sortOrder = _sortOrder ?? 0;


        return new ContentTypePropertyContainerModel
        {
            Key = key,
            ParentKey = parentKey,
            Name = name,
            Type = type,
            SortOrder = sortOrder,
        };
    }
}
