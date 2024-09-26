using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;

    public RelationTypePresentationFactory(IUmbracoMapper umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
    }

    public Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(IEnumerable<RelationItemModel> relationItemModels)
    {
        IReferenceResponseModel[] result = relationItemModels.Select(relationItemModel => relationItemModel.NodeType switch
        {
            Constants.UdiEntityType.Document => _umbracoMapper.Map<DocumentReferenceResponseModel>(relationItemModel),
            Constants.UdiEntityType.Media => _umbracoMapper.Map<MediaReferenceResponseModel>(relationItemModel),
            _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel) as IReferenceResponseModel,
        }).WhereNotNull().ToArray();

        return Task.FromResult<IEnumerable<IReferenceResponseModel>>(result);
    }
}
