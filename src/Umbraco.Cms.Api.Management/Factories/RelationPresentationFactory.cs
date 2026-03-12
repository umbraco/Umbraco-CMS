using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for representing relation data within the management API.
/// </summary>
public class RelationPresentationFactory : IRelationPresentationFactory
{
    private readonly IRelationService _relationService;
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationPresentationFactory"/> class with the specified services.
    /// </summary>
    /// <param name="relationService">An instance of <see cref="IRelationService"/> used to manage relations.</param>
    /// <param name="entityService">An instance of <see cref="IEntityService"/> used to manage entities.</param>
    public RelationPresentationFactory(IRelationService relationService, IEntityService entityService)
    {
        _relationService = relationService;
        _entityService = entityService;
    }

    /// <summary>
    /// Creates a <see cref="RelationResponseModel"/> from the specified <see cref="IRelation"/>,
    /// including parent and child entity references and their names if available.
    /// </summary>
    /// <param name="relation">The relation from which to create the response model.</param>
    /// <returns>
    /// A <see cref="RelationResponseModel"/> representing the relation, with references to the parent and child entities
    /// and their names populated if they can be resolved.
    /// </returns>
    public RelationResponseModel Create(IRelation relation)
    {
        IEntitySlim child = _entityService.Get(relation.ChildId)!;
        IEntitySlim parent = _entityService.Get(relation.ParentId)!;

        var relationResponseModel = new RelationResponseModel(
            new ReferenceByIdModel(relation.RelationType.Key),
            new RelationReferenceModel(parent.Key),
            new RelationReferenceModel(child.Key))
        {
            Id = relation.Key,
            Comment = relation.Comment,
            CreateDate = relation.CreateDate,
        };
        Tuple<IUmbracoEntity, IUmbracoEntity>? entities = _relationService.GetEntitiesFromRelation(relation);

        if (entities is not null)
        {
            relationResponseModel.Parent.Name = entities.Item1.Name;
            relationResponseModel.Child.Name = entities.Item2.Name;
        }

        return relationResponseModel;
    }

    /// <summary>
    /// Creates multiple <see cref="Umbraco.Cms.Api.Management.Models.RelationResponseModel"/> instances from the given relations.
    /// </summary>
    /// <param name="relations">The collection of relations to convert.</param>
    /// <returns>An enumerable of <see cref="Umbraco.Cms.Api.Management.Models.RelationResponseModel"/>.</returns>
    public IEnumerable<RelationResponseModel> CreateMultiple(IEnumerable<IRelation> relations) => relations.Select(Create);
}
