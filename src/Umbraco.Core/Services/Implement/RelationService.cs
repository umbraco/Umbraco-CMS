using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class RelationService : ScopeRepositoryService, IRelationService
    {
        private readonly IEntityService _entityService;
        private readonly IRelationRepository _relationRepository;
        private readonly IRelationTypeRepository _relationTypeRepository;

        public RelationService(IScopeProvider uowProvider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IEntityService entityService,
            IRelationRepository relationRepository, IRelationTypeRepository relationTypeRepository)
            : base(uowProvider, logger, eventMessagesFactory)
        {
            _relationRepository = relationRepository;
            _relationTypeRepository = relationTypeRepository;
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        /// <summary>
        /// Gets a <see cref="Relation"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="Relation"/></param>
        /// <returns>A <see cref="Relation"/> object</returns>
        public IRelation GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="RelationType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="RelationType"/></param>
        /// <returns>A <see cref="RelationType"/> object</returns>
        public IRelationType GetRelationTypeById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="RelationType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="RelationType"/></param>
        /// <returns>A <see cref="RelationType"/> object</returns>
        public IRelationType GetRelationTypeById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="RelationType"/> by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="RelationType"/></param>
        /// <returns>A <see cref="RelationType"/> object</returns>
        public IRelationType GetRelationTypeByAlias(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelationType>().Where(x => x.Alias == alias);
                return _relationTypeRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets all <see cref="Relation"/> objects
        /// </summary>
        /// <param name="ids">Optional array of integer ids to return relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetAllRelations(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Gets all <see cref="Relation"/> objects by their <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetAllRelationsByRelationType(RelationType relationType)
        {
            return GetAllRelationsByRelationType(relationType.Id);
        }

        /// <summary>
        /// Gets all <see cref="Relation"/> objects by their <see cref="RelationType"/>'s Id
        /// </summary>
        /// <param name="relationTypeId">Id of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetAllRelationsByRelationType(int relationTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
                return _relationRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all <see cref="Relation"/> objects
        /// </summary>
        /// <param name="ids">Optional array of integer ids to return relationtypes for</param>
        /// <returns>An enumerable list of <see cref="RelationType"/> objects</returns>
        public IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.GetMany(ids);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent Id
        /// </summary>
        /// <param name="id">Id of the parent to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByParentId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == id);
                return _relationRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent entity
        /// </summary>
        /// <param name="parent">Parent Entity to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent)
        {
            return GetByParentId(parent.Id);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their parent entity
        /// </summary>
        /// <param name="parent">Parent Entity to retrieve relations for</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias)
        {
            return GetByParent(parent).Where(relation => relation.RelationType.Alias == relationTypeAlias);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Id
        /// </summary>
        /// <param name="id">Id of the child to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByChildId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ChildId == id);
                return _relationRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Entity
        /// </summary>
        /// <param name="child">Child Entity to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByChild(IUmbracoEntity child)
        {
            return GetByChildId(child.Id);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child Entity
        /// </summary>
        /// <param name="child">Child Entity to retrieve relations for</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to retrieve</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias)
        {
            return GetByChild(child).Where(relation => relation.RelationType.Alias == relationTypeAlias);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by their child or parent Id.
        /// Using this method will get you all relations regards of it being a child or parent relation.
        /// </summary>
        /// <param name="id">Id of the child or parent to retrieve relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByParentOrChildId(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ChildId == id || x.ParentId == id);
                return _relationRepository.Get(query);
            }
        }

        public IEnumerable<IRelation> GetByParentOrChildId(int id, string relationTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var rtQuery = Query<IRelationType>().Where(x => x.Alias == relationTypeAlias);
                var relationType = _relationTypeRepository.Get(rtQuery).FirstOrDefault();
                if (relationType == null)
                    return Enumerable.Empty<IRelation>();

                var query = Query<IRelation>().Where(x => (x.ChildId == id || x.ParentId == id) && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Name of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeName">Name of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName)
        {
            List<int> relationTypeIds;
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelationType>().Where(x => x.Name == relationTypeName);
                var relationTypes = _relationTypeRepository.Get(query);
                relationTypeIds = relationTypes.Select(x => x.Id).ToList();
            }

            return relationTypeIds.Count == 0
                ? Enumerable.Empty<IRelation>()
                : GetRelationsByListOfTypeIds(relationTypeIds);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Alias of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeAlias">Alias of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias)
        {
            List<int> relationTypeIds;
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelationType>().Where(x => x.Alias == relationTypeAlias);
                var relationTypes = _relationTypeRepository.Get(query);
                relationTypeIds = relationTypes.Select(x => x.Id).ToList();
            }

            return relationTypeIds.Count == 0
                ? Enumerable.Empty<IRelation>()
                : GetRelationsByListOfTypeIds(relationTypeIds);
        }

        /// <summary>
        /// Gets a list of <see cref="Relation"/> objects by the Id of the <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationTypeId">Id of the <see cref="RelationType"/> to retrieve Relations for</param>
        /// <returns>An enumerable list of <see cref="Relation"/> objects</returns>
        public IEnumerable<IRelation> GetByRelationTypeId(int relationTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
                return _relationRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets the Child object from a Relation as an <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="relation">Relation to retrieve child object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public IUmbracoEntity GetChildEntityFromRelation(IRelation relation, bool loadBaseType = false)
        {
            var objectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ChildObjectType);
            return _entityService.Get(relation.ChildId, objectType, loadBaseType);
        }

        /// <summary>
        /// Gets the Parent object from a Relation as an <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="relation">Relation to retrieve parent object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        public IUmbracoEntity GetParentEntityFromRelation(IRelation relation, bool loadBaseType = false)
        {
            var objectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ParentObjectType);
            return _entityService.Get(relation.ParentId, objectType, loadBaseType);
        }

        /// <summary>
        /// Gets the Parent and Child objects from a Relation as a <see cref="Tuple"/>"/> with <see cref="IUmbracoEntity"/>.
        /// </summary>
        /// <param name="relation">Relation to retrieve parent and child object from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>Returns a Tuple with Parent (item1) and Child (item2)</returns>
        public Tuple<IUmbracoEntity, IUmbracoEntity> GetEntitiesFromRelation(IRelation relation, bool loadBaseType = false)
        {
            var childObjectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ChildObjectType);
            var parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ParentObjectType);

            var child = _entityService.Get(relation.ChildId, childObjectType, loadBaseType);
            var parent = _entityService.Get(relation.ParentId, parentObjectType, loadBaseType);

            return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
        }

        /// <summary>
        /// Gets the Child objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve child objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        public IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations, bool loadBaseType = false)
        {
            foreach (var relation in relations)
            {
                var objectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ChildObjectType);
                yield return _entityService.Get(relation.ChildId, objectType, loadBaseType);
            }
        }

        /// <summary>
        /// Gets the Parent objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve parent objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        public IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations, bool loadBaseType = false)
        {
            foreach (var relation in relations)
            {
                var objectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ParentObjectType);
                yield return _entityService.Get(relation.ParentId, objectType, loadBaseType);
            }
        }

        /// <summary>
        /// Gets the Parent and Child objects from a list of Relations as a list of <see cref="IUmbracoEntity"/> objects.
        /// </summary>
        /// <param name="relations">List of relations to retrieve parent and child objects from</param>
        /// <param name="loadBaseType">Optional bool to load the complete object graph when set to <c>False</c></param>
        /// <returns>An enumerable list of <see cref="Tuple"/> with <see cref="IUmbracoEntity"/></returns>
        public IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations, bool loadBaseType = false)
        {
            foreach (var relation in relations)
            {
                var childObjectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ChildObjectType);
                var parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.RelationType.ParentObjectType);

                var child = _entityService.Get(relation.ChildId, childObjectType, loadBaseType);
                var parent = _entityService.Get(relation.ParentId, parentObjectType, loadBaseType);

                yield return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
            }
        }

        /// <summary>
        /// Relates two objects by their entity Ids.
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <param name="childId">Id of the child</param>
        /// <param name="relationType">The type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        public IRelation Relate(int parentId, int childId, IRelationType relationType)
        {
            // Ensure that the RelationType has an indentity before using it to relate two entities
            if (relationType.HasIdentity == false)
                Save(relationType);

            var relation = new Relation(parentId, childId, relationType);

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IRelation>(relation);
                if (scope.Events.DispatchCancelable(SavingRelation, this, saveEventArgs))
                {
                    scope.Complete();
                    return relation; // fixme - returning sth that does not exist here?! // fixme - returning sth that does not exist here?!
                }

                _relationRepository.Save(relation);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelation, this, saveEventArgs);
                scope.Complete();
                return relation;
            }
        }

        /// <summary>
        /// Relates two objects that are based on the <see cref="IUmbracoEntity"/> interface.
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationType">The type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType)
        {
            return Relate(parent.Id, child.Id, relationType);
        }

        /// <summary>
        /// Relates two objects by their entity Ids.
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <param name="childId">Id of the child</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        public IRelation Relate(int parentId, int childId, string relationTypeAlias)
        {
            var relationType = GetRelationTypeByAlias(relationTypeAlias);
            if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
                throw new ArgumentNullException(string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));

            return Relate(parentId, childId, relationType);
        }

        /// <summary>
        /// Relates two objects that are based on the <see cref="IUmbracoEntity"/> interface.
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>The created <see cref="Relation"/></returns>
        public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias)
        {
            var relationType = GetRelationTypeByAlias(relationTypeAlias);
            if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
                throw new ArgumentNullException(string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));

            return Relate(parent.Id, child.Id, relationType);
        }

        /// <summary>
        /// Checks whether any relations exists for the passed in <see cref="RelationType"/>.
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to check for relations</param>
        /// <returns>Returns <c>True</c> if any relations exists for the given <see cref="RelationType"/>, otherwise <c>False</c></returns>
        public bool HasRelations(IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <summary>
        /// Checks whether any relations exists for the passed in Id.
        /// </summary>
        /// <param name="id">Id of an object to check relations for</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Id, otherwise <c>False</c></returns>
        public bool IsRelated(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == id || x.ChildId == id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parentId">Id of the Parent relation</param>
        /// <param name="childId">Id of the Child relation</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Ids, otherwise <c>False</c></returns>
        public bool AreRelated(int parentId, int childId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == parentId && x.ChildId == childId);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <summary>
        /// Checks whether two items are related with a given relation type alias
        /// </summary>
        /// <param name="parentId">Id of the Parent relation</param>
        /// <param name="childId">Id of the Child relation</param>
        /// <param name="relationTypeAlias">Alias of the relation type</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Ids and relation type, otherwise <c>False</c></returns>
        public bool AreRelated(int parentId, int childId, string relationTypeAlias)
        {
            var relType = GetRelationTypeByAlias(relationTypeAlias);
            if (relType == null)
                return false;

            return AreRelated(parentId, childId, relType);
        }


        /// <summary>
        /// Checks whether two items are related with a given relation type
        /// </summary>
        /// <param name="parentId">Id of the Parent relation</param>
        /// <param name="childId">Id of the Child relation</param>
        /// <param name="relationType">Type of relation</param>
        /// <returns>Returns <c>True</c> if any relations exists with the given Ids and relation type, otherwise <c>False</c></returns>
        public bool AreRelated(int parentId, int childId, IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == parentId && x.ChildId == childId && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
        public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child)
        {
            return AreRelated(parent.Id, child.Id);
        }

        /// <summary>
        /// Checks whether two items are related
        /// </summary>
        /// <param name="parent">Parent entity</param>
        /// <param name="child">Child entity</param>
        /// <param name="relationTypeAlias">Alias of the type of relation to create</param>
        /// <returns>Returns <c>True</c> if any relations exist between the entities, otherwise <c>False</c></returns>
        public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias)
        {
            return AreRelated(parent.Id, child.Id, relationTypeAlias);
        }


        /// <summary>
        /// Saves a <see cref="Relation"/>
        /// </summary>
        /// <param name="relation">Relation to save</param>
        public void Save(IRelation relation)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IRelation>(relation);
                if (scope.Events.DispatchCancelable(SavingRelation, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _relationRepository.Save(relation);
                scope.Complete();
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelation, this, saveEventArgs);
            }
        }

        /// <summary>
        /// Saves a <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType">RelationType to Save</param>
        public void Save(IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IRelationType>(relationType);
                if (scope.Events.DispatchCancelable(SavingRelationType, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _relationTypeRepository.Save(relationType);
                scope.Complete();
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelationType, this, saveEventArgs);
            }
        }

        /// <summary>
        /// Deletes a <see cref="Relation"/>
        /// </summary>
        /// <param name="relation">Relation to Delete</param>
        public void Delete(IRelation relation)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IRelation>(relation);
                if (scope.Events.DispatchCancelable(DeletingRelation, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _relationRepository.Delete(relation);
                scope.Complete();
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedRelation, this, deleteEventArgs);
            }
        }

        /// <summary>
        /// Deletes a <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType">RelationType to Delete</param>
        public void Delete(IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IRelationType>(relationType);
                if (scope.Events.DispatchCancelable(DeletingRelationType, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _relationTypeRepository.Delete(relationType);
                scope.Complete();
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(DeletedRelationType, this, deleteEventArgs);
            }
        }

        /// <summary>
        /// Deletes all <see cref="Relation"/> objects based on the passed in <see cref="RelationType"/>
        /// </summary>
        /// <param name="relationType"><see cref="RelationType"/> to Delete Relations for</param>
        public void DeleteRelationsOfType(IRelationType relationType)
        {
            var relations = new List<IRelation>();
            using (var scope = ScopeProvider.CreateScope())
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
                relations.AddRange(_relationRepository.Get(query).ToList());

                foreach (var relation in relations)
                    _relationRepository.Delete(relation);

                scope.Complete();

                scope.Events.Dispatch(DeletedRelation, this, new DeleteEventArgs<IRelation>(relations, false));
            }
        }

        #region Private Methods

        private IEnumerable<IRelation> GetRelationsByListOfTypeIds(IEnumerable<int> relationTypeIds)
        {
            var relations = new List<IRelation>();
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                foreach (var relationTypeId in relationTypeIds)
                {
                    var id = relationTypeId;
                    var query = Query<IRelation>().Where(x => x.RelationTypeId == id);
                    relations.AddRange(_relationRepository.Get(query));
                }
            }
            return relations;
        }
        #endregion

        #region Events Handlers
        /// <summary>
        /// Occurs before Deleting a Relation
        /// </summary>
        public static event TypedEventHandler<IRelationService, DeleteEventArgs<IRelation>> DeletingRelation;

        /// <summary>
        /// Occurs after a Relation is Deleted
        /// </summary>
        public static event TypedEventHandler<IRelationService, DeleteEventArgs<IRelation>> DeletedRelation;

        /// <summary>
        /// Occurs before Saving a Relation
        /// </summary>
        public static event TypedEventHandler<IRelationService, SaveEventArgs<IRelation>> SavingRelation;

        /// <summary>
        /// Occurs after a Relation is Saved
        /// </summary>
        public static event TypedEventHandler<IRelationService, SaveEventArgs<IRelation>> SavedRelation;

        /// <summary>
        /// Occurs before Deleting a RelationType
        /// </summary>
        public static event TypedEventHandler<IRelationService, DeleteEventArgs<IRelationType>> DeletingRelationType;

        /// <summary>
        /// Occurs after a RelationType is Deleted
        /// </summary>
        public static event TypedEventHandler<IRelationService, DeleteEventArgs<IRelationType>> DeletedRelationType;

        /// <summary>
        /// Occurs before Saving a RelationType
        /// </summary>
        public static event TypedEventHandler<IRelationService, SaveEventArgs<IRelationType>> SavingRelationType;

        /// <summary>
        /// Occurs after a RelationType is Saved
        /// </summary>
        public static event TypedEventHandler<IRelationService, SaveEventArgs<IRelationType>> SavedRelationType;
        #endregion
    }
}
