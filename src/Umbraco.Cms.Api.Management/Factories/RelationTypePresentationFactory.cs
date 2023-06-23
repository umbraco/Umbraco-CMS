using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IShortStringHelper _shortStringHelper;

    public RelationTypePresentationFactory(IShortStringHelper shortStringHelper) => _shortStringHelper = shortStringHelper;

    public IRelationType CreateRelationType(CreateRelationTypeRequestModel createRelationTypeRequestModel) =>
        new RelationType(
            createRelationTypeRequestModel.Name,
            createRelationTypeRequestModel.Name.ToSafeAlias(_shortStringHelper, true),
            createRelationTypeRequestModel.IsBidirectional,
            createRelationTypeRequestModel.ParentObjectType,
            createRelationTypeRequestModel.ChildObjectType,
            createRelationTypeRequestModel.IsDependency,
            createRelationTypeRequestModel.Id);

    public void MapUpdateModelToRelationType(UpdateRelationTypeRequestModel updateRelationTypeRequestModel, IRelationType target)
    {
        target.Name = updateRelationTypeRequestModel.Name;
        target.Alias = updateRelationTypeRequestModel.Name.ToSafeAlias(_shortStringHelper, true);
        target.ChildObjectType = updateRelationTypeRequestModel.ChildObjectType;
        target.IsBidirectional = updateRelationTypeRequestModel.IsBidirectional;
        if (target is IRelationTypeWithIsDependency targetWithIsDependency)
        {
            targetWithIsDependency.IsDependency = updateRelationTypeRequestModel.IsDependency;
        }

        target.ParentObjectType = updateRelationTypeRequestModel.ParentObjectType;
    }
}
