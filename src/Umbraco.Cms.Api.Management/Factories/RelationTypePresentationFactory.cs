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

/// <inheritdoc />
public class RelationTypePresentationFactory : IRelationTypePresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IEntityRepository _entityRepository;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IElementPresentationFactory _elementPresentationFactory;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypePresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The Umbraco mapper.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="documentPresentationFactory">The document presentation factory.</param>
    /// <param name="elementPresentationFactory">The element presentation factory.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    public RelationTypePresentationFactory(
        IUmbracoMapper umbracoMapper,
        IEntityRepository entityRepository,
        IDocumentPresentationFactory documentPresentationFactory,
        IElementPresentationFactory elementPresentationFactory,
        IScopeProvider scopeProvider)
    {
        _umbracoMapper = umbracoMapper;
        _entityRepository = entityRepository;
        _documentPresentationFactory = documentPresentationFactory;
        _elementPresentationFactory = elementPresentationFactory;
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypePresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The Umbraco mapper.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="documentPresentationFactory">The document presentation factory.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
    public RelationTypePresentationFactory(
        IUmbracoMapper umbracoMapper,
        IEntityRepository entityRepository,
        IDocumentPresentationFactory documentPresentationFactory,
        IScopeProvider scopeProvider)
        : this(
            umbracoMapper,
            entityRepository,
            documentPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IElementPresentationFactory>(),
            scopeProvider)
    {
    }

    /// <inheritdoc />
    public Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(
        IEnumerable<RelationItemModel> relationItemModels)
    {
        IReadOnlyCollection<RelationItemModel> relationItemModelsCollection = relationItemModels.ToArray();

        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

        List<IEntitySlim> documentSlimEntities = GetSlimEntities(
            relationItemModelsCollection,
            Constants.UdiEntityType.Document,
            Constants.ObjectTypes.Document);
        List<IEntitySlim> elementSlimEntities = GetSlimEntities(
            relationItemModelsCollection,
            Constants.UdiEntityType.Element,
            Constants.ObjectTypes.Element);

        IReferenceResponseModel[] result = relationItemModelsCollection.Select<RelationItemModel, IReferenceResponseModel?>(relationItemModel =>
            relationItemModel.NodeType switch
            {
                Constants.ReferenceType.Document => MapReference<DocumentReferenceResponseModel, DocumentEntitySlim>(
                    relationItemModel,
                    documentSlimEntities,
                    (r, e) => r.Variants = _documentPresentationFactory.CreateVariantsItemResponseModels(e)),
                Constants.ReferenceType.Element => MapReference<ElementReferenceResponseModel, IElementEntitySlim>(
                    relationItemModel,
                    elementSlimEntities,
                    (r, e) => r.Variants = _elementPresentationFactory.CreateVariantsItemResponseModels(e)),
                Constants.ReferenceType.ElementContainer => _umbracoMapper.Map<ElementContainerReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.Media => _umbracoMapper.Map<MediaReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.Member => _umbracoMapper.Map<MemberReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.DocumentTypePropertyType => _umbracoMapper.Map<DocumentTypePropertyTypeReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MediaTypePropertyType => _umbracoMapper.Map<MediaTypePropertyTypeReferenceResponseModel>(relationItemModel),
                Constants.ReferenceType.MemberTypePropertyType => _umbracoMapper.Map<MemberTypePropertyTypeReferenceResponseModel>(relationItemModel),
                _ => _umbracoMapper.Map<DefaultReferenceResponseModel>(relationItemModel),
            }).WhereNotNull().ToArray();

        return Task.FromResult<IEnumerable<IReferenceResponseModel>>(result);
    }

    private TResponse? MapReference<TResponse, TEntity>(
        RelationItemModel relationItemModel,
        List<IEntitySlim> slimEntities,
        Action<TResponse, TEntity> enrichResponse)
        where TResponse : class
        where TEntity : class, IEntitySlim
    {
        TResponse? responseModel = _umbracoMapper.Map<TResponse>(relationItemModel);
        if (responseModel is null
            || slimEntities.FirstOrDefault(e => e.Key == relationItemModel.NodeKey) is not TEntity matchingEntity)
        {
            return responseModel;
        }

        enrichResponse(responseModel, matchingEntity);
        return responseModel;
    }

    private List<IEntitySlim> GetSlimEntities(
        IReadOnlyCollection<RelationItemModel> relationItemModels,
        string nodeType,
        Guid objectType)
    {
        Guid[] keys = relationItemModels
            .Where(item => item.NodeType == nodeType)
            .Select(item => item.NodeKey)
            .Distinct()
            .ToArray();

        if (keys.Length == 0)
        {
            return [];
        }

        return _entityRepository.GetAll(objectType, keys).ToList();
    }
}
