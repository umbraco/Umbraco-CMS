using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.MemberType}")]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMembersOrMemberTypes)]
public class MemberTypeTreeControllerBase : FolderTreeControllerBase<MemberTypeTreeItemResponseModel>
{
    private readonly IMemberTypeService _memberTypeService;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public MemberTypeTreeControllerBase(IEntityService entityService, IMemberTypeService memberTypeService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              memberTypeService)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public MemberTypeTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            memberTypeService)
    {
    }

    public MemberTypeTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap) =>
        _memberTypeService = memberTypeService;


    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MemberType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.MemberTypeContainer;

    protected override MemberTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var memberTypes = _memberTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            MemberTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (memberTypes.TryGetValue(entity.Id, out IMemberType? memberType))
            {
                responseModel.Icon = memberType.Icon ?? responseModel.Icon;
            }

            return responseModel;
        }).ToArray();
    }
}
