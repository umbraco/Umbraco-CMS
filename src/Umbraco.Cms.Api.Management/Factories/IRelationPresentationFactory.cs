using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating models used to present relations in the API.
/// </summary>
public interface IRelationPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="RelationResponseModel"/> from the given <see cref="IRelation"/> instance.
    /// </summary>
    /// <param name="relation">The relation instance to create the response model from.</param>
    /// <returns>A <see cref="RelationResponseModel"/> representing the provided relation.</returns>
    RelationResponseModel Create(IRelation relation);

    /// <summary>
    /// Creates multiple <see cref="RelationResponseModel"/> instances from the given <see cref="IRelation"/> collection.
    /// </summary>
    /// <param name="relations">The collection of relations to convert.</param>
    /// <returns>An enumerable of <see cref="RelationResponseModel"/> representing the provided relations.</returns>
    IEnumerable<RelationResponseModel> CreateMultiple(IEnumerable<IRelation> relations);
}
