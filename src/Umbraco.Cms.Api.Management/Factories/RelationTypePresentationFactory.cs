using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IEntityRepository _entityRepository;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IScopeProvider _scopeProvider;

    [Obsolete("Please use the non obsoleted constructor. Scheduled for removal in v17")]
    public RelationTypePresentationFactory(IUmbracoMapper umbracoMapper)
        : this(
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IEntityRepository>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentPresentationFactory>(),
            StaticServiceProvider.Instance.GetRequiredService<IScopeProvider>())
    {
    }

    public RelationTypePresentationFactory(
        IUmbracoMapper umbracoMapper,
        IEntityRepository entityRepository,
        IDocumentPresentationFactory documentPresentationFactory,
        IScopeProvider scopeProvider)
    {
        _umbracoMapper = umbracoMapper;
        _entityRepository = entityRepository;
        _documentPresentationFactory = documentPresentationFactory;
        _scopeProvider = scopeProvider;
    }

    public async Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(
        IEnumerable<RelationItemModel> relationItemModels)
    {
        IReadOnlyCollection<RelationItemModel> relationItemModelsCollection = relationItemModels.ToArray();

        Guid[] documentKeys = relationItemModelsCollection
            .Where(item => item.NodeType is Constants.UdiEntityType.Document)
            .Select(item => item.NodeKey).Distinct().ToArray();

        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

        var slimEntities = _entityRepository.GetAll(Constants.ObjectTypes.Document, documentKeys).ToList();

        IReferenceResponseModel[] result = relationItemModelsCollection.Select(relationItemModel =>
            relationItemModel.NodeType switch
            {
                Constants.ReferenceType.Document => MapDocumentReference(relationItemModel, slimEntities),
                Constants.ReferenceType.Media => _umbracoMapper.Map<MediaReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.Member => _umbracoMapper.Map<MemberReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.DocumentTypeProperty => _umbracoMapper.Map<DocumentTypePropertyReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MediaTypeProperty => _umbracoMapper.Map<MediaTypePropertyReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MemberTypeProperty => _umbracoMapper.Map<MemberTypePropertyReferenceResponseModel>(relationItemModel),
                _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel),
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
