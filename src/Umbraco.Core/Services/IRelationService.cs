using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Services
{
    public interface IRelationService : IService
    {
        /// <summary>
        /// Gets a <see cref="Relation"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="Relation"/></param>
        /// <returns>A <see cref="Relation"/> object</returns>
        IRelation GetById(int id);

        /// <summary>
        /// Gets a <see cref="RelationType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="RelationType"/></param>
        /// <returns>A <see cref="RelationType"/> object</returns>
        IRelationType GetRelationTypeById(int id);

        /// <summary>
        /// Gets a <see cref="RelationType"/> by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="RelationType"/></param>
        /// <returns>A <see cref="RelationType"/> object</returns>
        IRelationType GetRelationTypeByAlias(string alias);

        /// <summary>
        /// Gets all <see cref="Relation"/> objects
        /// </summary>
        /// <param name="ids">Optional array of integer ids to return relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetAllRelations(params int[] ids);

        /// <summary>
        /// Gets all <see cref="Relation"/> objects by their <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetAllRelationsByRelationType(RelationType relationType);

        /// <summary>
        /// Gets all <see cref="Relation"/> objects by their <see cref="RelationType"/>'s Id
        /// </summary>
        /// <param name="relationTypeId">Id of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetAllRelationsByRelationType(int relationTypeId);

        /// <summary>
        /// Gets all <see cref="Relation"/> objects
        /// </summary>
        /// <param name="ids">Optional array of integer ids to return relationtypes for</param>
        /// <returns>An enumerable list of <see cref="RelationType"/> objects</returns>
        IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent Id
        /// </summary>
        /// <param name="id">Id of the parent to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByParentId(int id);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent entity
        /// </summary>
        /// <param name="parent">Parent Entity to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByParent(IUmbracoEntity parent);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent entity
        /// </summary>
        /// <param name="parent">Parent Entity to retrieve relations for</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Id
        /// </summary>
        /// <param name="id">Id of the child to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByChildId(int id);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Entity
        /// </summary>
        /// <param name="child">Child Entity to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByChild(IUmbracoEntity child);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Entity
        /// </summary>
        /// <param name="child">Child Entity to retrieve relations for</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child or parent Id.
        /// Using this method will get you all relations regards of it being a child or parent relation.
        /// </summary>
        /// <param name="id">Id of the child or parent to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByParentOrChildId(int id);

        IEnumerable<IRelation> GetByParentOrChildId(int id, string relationTypeAlias);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Name of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeName">Name of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Alias of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeAlias">Alias of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias);

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Id of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeId">Id of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        IEnumerable<IRelation> GetByRelationTypeId(int relationTypeId);

        /// <summary>
        /// Gets the Child object from a Relation as an <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="relation">Relation to retrieve child object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetChildEntityFromRelation(IRelation relation, bool loadBaseType = false);

        /// <summary>
        /// Gets the Parent object from a Relation as an <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="relation">Relation to retrieve parent object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetParentEntityFromRelation(IRelation relation, bool loadBaseType = false);

        /// <summary>
        /// Gets the Parent and Child objects from a Relation as a <see cref="Tuple"/>"/> with <see cref="IUmbracoEntity"/>.
        /// </summary>
        /// <param name="relation">Relation to retrieve parent and child object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>Returns a Tuple with Parent (item1) and Child (item2)</returns>
        Tuple<IUmbracoEntity, IUmbracoEntity> GetEntitiesFromRelation(IRelation relation, bool loadBaseType = false);

        /// <summary>
        /// Gets the Child objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve child objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations, bool loadBaseType = false);

        /// <summary>
        /// Gets the Parent objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve parent objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations,
                                                                   bool loadBaseType = false);

        /// <summary>
        /// Gets the Parent and Child objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve parent and child objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="Tuple"/> with <see cref="IUmbracoEntity"/></returns>
        IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(
            IEnumerable<IRelation> relations,
            bool loadBaseType = false);

        /// <summary>
        /// Relates two objects that are based on the <see cref="IUmbracoEntity"/> interface.
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationType">The type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType);

        /// <summary>
        /// Relates two objects that are based on the <see cref="IUmbracoEntity"/> interface.
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

        /// <summary>
        /// Checks whether any relations exists for the passed in <see cref="RelationType"/>.
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to check for relations</param>
        /// <returns>Returns <c>True</c> if any relations exists for the given <see cref="RelationType"/>, otherwise <c>False</c></returns>
        bool HasRelations(IRelationType relationType);

        /// <summary>
        /// Checks whether any relations exists for the passed in Id.
        /// </summary>
        /// <param name="id">Id of an object to check relations for</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Id, otherwise <c>False</c></returns>
        bool IsRelated(int id);

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parentId">Id of the Parent relation</param>
        /// <param name="childId">Id of the Child relation</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Ids, otherwise <c>False</c></returns>
        bool AreRelated(int parentId, int childId);

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
        bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child);

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
        bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parentId">Id of the Parent relation</param>
        /// <param name="childId">Id of the Child relation</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
        bool AreRelated(int parentId, int childId, string relationTypeAlias);

        /// <summary>
        /// Saves a <see cref="Relation"/>
        /// </summary>
        /// <param name="relation">Relation to save</param>
        void Save(IRelation relation);

        /// <summary>
        /// Saves a <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType">RelationType to Save</param>
        void Save(IRelationType relationType);

        /// <summary>
        /// Deletes a <see cref="Relation"/>
        /// </summary>
        /// <param name="relation">Relation to Delete</param>
        void Delete(IRelation relation);

        /// <summary>
        /// Deletes a <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType">RelationType to Delete</param>
        void Delete(IRelationType relationType);

        /// <summary>
        /// Deletes all <see cref="Relation"/> objects based on the passed in <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to Delete Relations for</param>
        void DeleteRelationsOfType(IRelationType relationType);
    }
}