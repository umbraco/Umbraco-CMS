using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Provides a base controller for managing tree operations on entities that have a name property.
/// This controller is intended to be inherited by controllers handling hierarchical structures of named entities.
/// </summary>
public abstract class NamedEntityTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : NamedEntityTreeItemResponseModel, new()
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected NamedEntityTreeControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected NamedEntityTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders)
        : base(entityService, flagProviders)
    {
    }

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem item = base.MapTreeItemViewModel(parentKey, entity);
        item.Name = entity.Name ?? string.Empty;
        return item;
    }
}
