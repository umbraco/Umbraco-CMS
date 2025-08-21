using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class NamedEntityTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : NamedEntityTreeItemResponseModel, new()
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected NamedEntityTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected NamedEntityTreeControllerBase(IEntityService entityService, SignProviderCollection signProviders)
        : base(entityService, signProviders)
    {
    }

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem item = base.MapTreeItemViewModel(parentKey, entity);
        item.Name = entity.Name ?? string.Empty;
        return item;
    }
}
