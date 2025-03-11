using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IEntityRepository _entityRepository;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    [Obsolete("Please use the non obsoleted constructor. Scheduled for removal in v17")]
    public RelationTypePresentationFactory(IUmbracoMapper umbracoMapper)
        : this(
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IEntityRepository>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentPresentationFactory>())
    {
    }

    public RelationTypePresentationFactory(
        IUmbracoMapper umbracoMapper,
        IEntityRepository entityRepository,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _umbracoMapper = umbracoMapper;
        _entityRepository = entityRepository;
        _documentPresentationFactory = documentPresentationFactory;
    }

    public async Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(
        IEnumerable<RelationItemModel> relationItemModels)
    {
        Guid[] documentKeys = relationItemModels
            .Where(item => item.NodeType is Constants.UdiEntityType.Document)
            .Select(item => item.NodeKey).Distinct().ToArray();

        var slimEntities = _entityRepository.GetAll(Constants.ObjectTypes.Document, documentKeys).ToList();

        IReferenceResponseModel[] result = relationItemModels.Select(relationItemModel =>
            relationItemModel.NodeType switch
            {
                Constants.UdiEntityType.Document => MapDocumentReference(relationItemModel, slimEntities),
                Constants.UdiEntityType.Media => _umbracoMapper.Map<MediaReferenceResponseModel>(relationItemModel),
                _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel) as IReferenceResponseModel,
            }).WhereNotNull().ToArray();

        return await Task.FromResult(result);
    }

    private IReferenceResponseModel? MapDocumentReference(RelationItemModel relationItemModel,
        List<IEntitySlim> slimEntities)
    {
        DocumentReferenceResponseModel? documentReferenceResponseModel =
            _umbracoMapper.Map<DocumentReferenceResponseModel>(relationItemModel);
        if (documentReferenceResponseModel is not null
            && slimEntities.FirstOrDefault(e => e.Key == relationItemModel.NodeKey) is DocumentEntitySlim
                matchingSlimDocument)
        {
            documentReferenceResponseModel.Variants =
                _documentPresentationFactory.CreateVariantsItemResponseModels(matchingSlimDocument);
        }

        return documentReferenceResponseModel;
    }
}
