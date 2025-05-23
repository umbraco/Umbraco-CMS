using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class NamedEntityTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : NamedEntityTreeItemResponseModel, new()
{
    protected NamedEntityTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem item = base.MapTreeItemViewModel(parentKey, entity);
        item.Name = entity.Name ?? string.Empty;
        return item;
    }
}
