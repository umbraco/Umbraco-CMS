using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

public interface IRelationService : IService
{
    /// <summary>
    ///     Gets a <see cref="IRelation" /> by its Id
    /// </summary>
    /// <param name="id">Id of the <see cref="IRelation" /></param>
    /// <returns>A <see cref="IRelation" /> object</returns>
    IRelation? GetById(int id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Id
    /// </summary>
    /// <param name="id">Id of the <see cref="IRelationType" /></param>
    /// <returns>A <see cref="IRelationType" /> object</returns>
    IRelationType? GetRelationTypeById(int id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Id
    /// </summary>
    /// <param name="id">Id of the <see cref="IRelationType" /></param>
    /// <returns>A <see cref="IRelationType" /> object</returns>
    IRelationType? GetRelationTypeById(Guid id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="IRelationType" /></param>
    /// <returns>A <see cref="IRelationType" /> object</returns>
    IRelationType? GetRelationTypeByAlias(string alias);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects
    /// </summary>
    /// <param name="ids">Optional array of integer ids to return relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetAllRelations(params int[] ids);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects by their <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationType"><see cref="IRelation" /> to retrieve Relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetAllRelationsByRelationType(IRelationType relationType);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects by their <see cref="IRelationType" />'s Id
    /// </summary>
    /// <param name="relationTypeId">Id of the <see cref="IRelationType" /> to retrieve Relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetAllRelationsByRelationType(int relationTypeId);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects
    /// </summary>
    /// <param name="ids">Optional array of integer ids to return relationtypes for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent Id
    /// </summary>
    /// <param name="id">Id of the parent to retrieve relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetByParentId(int id);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent Id
    /// </summary>
    /// <param name="id">Id of the parent to retrieve relations for</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetByParentId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent entity
    /// </summary>
    /// <param name="parent">Parent Entity to retrieve relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetByParent(IUmbracoEntity parent);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent entity
    /// </summary>
    /// <param name="parent">Parent Entity to retrieve relations for</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child Id
    /// </summary>
    /// <param name="id">Id of the child to retrieve relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByChildId(int id);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child Id
    /// </summary>
    /// <param name="id">Id of the child to retrieve relations for</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByChildId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child Entity
    /// </summary>
    /// <param name="child">Child Entity to retrieve relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByChild(IUmbracoEntity child);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child Entity
    /// </summary>
    /// <param name="child">Child Entity to retrieve relations for</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child or parent Id.
    ///     Using this method will get you all relations regards of it being a child or parent relation.
    /// </summary>
    /// <param name="id">Id of the child or parent to retrieve relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByParentOrChildId(int id);

    IEnumerable<IRelation> GetByParentOrChildId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a relation by the unique combination of parentId, childId and relationType.
    /// </summary>
    /// <param name="parentId">The id of the parent item.</param>
    /// <param name="childId">The id of the child item.</param>
    /// <param name="relationType">The RelationType.</param>
    /// <returns>The relation or null</returns>
    IRelation? GetByParentAndChildId(int parentId, int childId, IRelationType relationType);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the Name of the <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationTypeName">Name of the <see cref="IRelationType" /> to retrieve Relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the Alias of the <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationTypeAlias">Alias of the <see cref="IRelationType" /> to retrieve Relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the Id of the <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationTypeId">Id of the <see cref="IRelationType" /> to retrieve Relations for</param>
    /// <returns>An enumerable list of <see cref="IRelation" /> objects</returns>
    IEnumerable<IRelation>? GetByRelationTypeId(int relationTypeId);

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" />
    /// </summary>
    /// <param name="relationTypeId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalChildren"></param>
    /// <returns></returns>
    IEnumerable<IRelation> GetPagedByRelationTypeId(int relationTypeId, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering = null);

    /// <summary>
    ///     Gets the Child object from a Relation as an <see cref="IUmbracoEntity" />
    /// </summary>
    /// <param name="relation">Relation to retrieve child object from</param>
    /// <returns>An <see cref="IUmbracoEntity" /></returns>
    IUmbracoEntity? GetChildEntityFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Parent object from a Relation as an <see cref="IUmbracoEntity" />
    /// </summary>
    /// <param name="relation">Relation to retrieve parent object from</param>
    /// <returns>An <see cref="IUmbracoEntity" /></returns>
    IUmbracoEntity? GetParentEntityFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Parent and Child objects from a Relation as a <see cref="Tuple" />"/> with <see cref="IUmbracoEntity" />.
    /// </summary>
    /// <param name="relation">Relation to retrieve parent and child object from</param>
    /// <returns>Returns a Tuple with Parent (item1) and Child (item2)</returns>
    Tuple<IUmbracoEntity, IUmbracoEntity>? GetEntitiesFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Child objects from a list of Relations as a list of <see cref="IUmbracoEntity" /> objects.
    /// </summary>
    /// <param name="relations">List of relations to retrieve child objects from</param>
    /// <returns>An enumerable list of <see cref="IUmbracoEntity" /></returns>
    IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Gets the Parent objects from a list of Relations as a list of <see cref="IUmbracoEntity" /> objects.
    /// </summary>
    /// <param name="relations">List of relations to retrieve parent objects from</param>
    /// <returns>An enumerable list of <see cref="IUmbracoEntity" /></returns>
    IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Returns paged parent entities for a related child id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalChildren"></param>
    /// <returns>An enumerable list of <see cref="IUmbracoEntity" /></returns>
    IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes);

    /// <summary>
    ///     Returns paged child entities for a related parent id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalChildren"></param>
    /// <returns>An enumerable list of <see cref="IUmbracoEntity" /></returns>
    IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes);

    /// <summary>
    ///     Gets the Parent and Child objects from a list of Relations as a list of <see cref="IUmbracoEntity" /> objects.
    /// </summary>
    /// <param name="relations">List of relations to retrieve parent and child objects from</param>
    /// <returns>An enumerable list of <see cref="Tuple" /> with <see cref="IUmbracoEntity" /></returns>
    IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Relates two objects by their entity Ids.
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <param name="childId">Id of the child</param>
    /// <param name="relationType">The type of relation to create</param>
    /// <returns>The created <see cref="IRelation" /></returns>
    IRelation Relate(int parentId, int childId, IRelationType relationType);

    /// <summary>
    ///     Relates two objects that are based on the <see cref="IUmbracoEntity" /> interface.
    /// </summary>
    /// <param name="parent">Parent entity</param>
    /// <param name="child">Child entity</param>
    /// <param name="relationType">The type of relation to create</param>
    /// <returns>The created <see cref="IRelation" /></returns>
    IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType);

    /// <summary>
    ///     Relates two objects by their entity Ids.
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <param name="childId">Id of the child</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
    /// <returns>The created <see cref="IRelation" /></returns>
    IRelation Relate(int parentId, int childId, string relationTypeAlias);

    /// <summary>
    ///     Relates two objects that are based on the <see cref="IUmbracoEntity" /> interface.
    /// </summary>
    /// <param name="parent">Parent entity</param>
    /// <param name="child">Child entity</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
    /// <returns>The created <see cref="IRelation" /></returns>
    IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Checks whether any relations exists for the passed in <see cref="IRelationType" />.
    /// </summary>
    /// <param name="relationType"><see cref="IRelationType" /> to check for relations</param>
    /// <returns>
    ///     Returns <c>True</c> if any relations exists for the given <see cref="IRelationType" />, otherwise <c>False</c>
    /// </returns>
    bool HasRelations(IRelationType relationType);

    /// <summary>
    ///     Checks whether any relations exists for the passed in Id.
    /// </summary>
    /// <param name="id">Id of an object to check relations for</param>
    /// <returns>Returns <c>True</c> if any relations exists with the given Id, otherwise <c>False</c></returns>
    bool IsRelated(int id);

    /// <summary>
    ///     Checks whether two items are related
    /// </summary>
    /// <param name="parentId">Id of the Parent relation</param>
    /// <param name="childId">Id of the Child relation</param>
    /// <returns>Returns <c>True</c> if any relations exists with the given Ids, otherwise <c>False</c></returns>
    bool AreRelated(int parentId, int childId);

    /// <summary>
    ///     Checks whether two items are related
    /// </summary>
    /// <param name="parent">Parent entity</param>
    /// <param name="child">Child entity</param>
    /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
    bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child);

    /// <summary>
    ///     Checks whether two items are related
    /// </summary>
    /// <param name="parent">Parent entity</param>
    /// <param name="child">Child entity</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
    /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
    bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Checks whether two items are related
    /// </summary>
    /// <param name="parentId">Id of the Parent relation</param>
    /// <param name="childId">Id of the Child relation</param>
    /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
    /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
    bool AreRelated(int parentId, int childId, string relationTypeAlias);

    /// <summary>
    ///     Saves a <see cref="IRelation" />
    /// </summary>
    /// <param name="relation">Relation to save</param>
    void Save(IRelation relation);

    void Save(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Saves a <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationType">RelationType to Save</param>
    void Save(IRelationType relationType);

    /// <summary>
    ///     Deletes a <see cref="IRelation" />
    /// </summary>
    /// <param name="relation">Relation to Delete</param>
    void Delete(IRelation relation);

    /// <summary>
    ///     Deletes a <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationType">RelationType to Delete</param>
    void Delete(IRelationType relationType);

    /// <summary>
    ///     Deletes all <see cref="IRelation" /> objects based on the passed in <see cref="IRelationType" />
    /// </summary>
    /// <param name="relationType"><see cref="IRelationType" /> to Delete Relations for</param>
    void DeleteRelationsOfType(IRelationType relationType);
}
