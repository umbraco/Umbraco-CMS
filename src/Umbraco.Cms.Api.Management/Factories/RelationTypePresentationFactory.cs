using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    public RelationTypePresentationFactory(IShortStringHelper shortStringHelper, IUmbracoMapper umbracoMapper)
    {
        _shortStringHelper = shortStringHelper;
        _umbracoMapper = umbracoMapper;
    }

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

    public async Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(IEnumerable<RelationItemModel> relationItemModels)
    {
        IReferenceResponseModel[] result = relationItemModels.Select(relationItemModel => relationItemModel.NodeType switch
        {
            Constants.UdiEntityType.Document => _umbracoMapper.Map<DocumentReferenceResponseModel>(relationItemModel),
            Constants.UdiEntityType.Media => _umbracoMapper.Map<MediaReferenceResponseModel>(relationItemModel),
            _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel) as IReferenceResponseModel,
        }).WhereNotNull().ToArray();

        return await Task.FromResult(result);
    }
}
