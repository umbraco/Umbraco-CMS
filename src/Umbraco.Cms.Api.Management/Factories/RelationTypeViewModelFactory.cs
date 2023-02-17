using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypeViewModelFactory : IRelationTypeViewModelFactory
{
    private readonly IShortStringHelper _shortStringHelper;

    public RelationTypeViewModelFactory(IShortStringHelper shortStringHelper) => _shortStringHelper = shortStringHelper;

    public RelationType CreateRelationType(RelationTypeSavingViewModel relationTypeSavingViewModel) =>
        new(
            relationTypeSavingViewModel.Name,
            relationTypeSavingViewModel.Name.ToSafeAlias(_shortStringHelper, true),
            relationTypeSavingViewModel.IsBidirectional,
            relationTypeSavingViewModel.ParentObjectType,
            relationTypeSavingViewModel.ChildObjectType,
            relationTypeSavingViewModel.IsDependency,
            relationTypeSavingViewModel.Key);
}
