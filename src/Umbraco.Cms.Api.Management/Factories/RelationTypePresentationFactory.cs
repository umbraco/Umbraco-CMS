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

/// <summary>
/// Provides methods to create presentation models for relation types in the management API.
/// </summary>
public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IEntityRepository _entityRepository;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypePresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">Maps entities to models.</param>
    /// <param name="entityRepository">Provides access to entity data.</param>
    /// <param name="documentPresentationFactory">Creates document presentations.</param>
    /// <param name="scopeProvider">Manages database scopes.</param>
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

    /// <summary>
    /// Asynchronously creates a collection of reference response models from the provided relation item models.
    /// </summary>
    /// <param name="relationItemModels">A collection of <see cref="RelationItemModel"/> instances to be mapped to reference response models.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{IReferenceResponseModel}"/>,
    /// where each item is a mapped reference response model corresponding to the input relation item models.
    /// </returns>
    public Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(
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
                Constants.ReferenceType.DocumentTypePropertyType => _umbracoMapper.Map<DocumentTypePropertyTypeReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MediaTypePropertyType => _umbracoMapper.Map<MediaTypePropertyTypeReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MemberTypePropertyType => _umbracoMapper.Map<MemberTypePropertyTypeReferenceResponseModel>(relationItemModel),
                _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel),
            }).WhereNotNull().ToArray();

        return Task.FromResult<IEnumerable<IReferenceResponseModel>>(result);
    }

    private IReferenceResponseModel? MapDocumentReference(
        RelationItemModel relationItemModel,
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
