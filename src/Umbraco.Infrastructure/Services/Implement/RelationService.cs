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
        private readonly IAuditRepository _auditRepository;

        public RelationService(IScopeProvider uowProvider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IEntityService entityService,
            IRelationRepository relationRepository, IRelationTypeRepository relationTypeRepository, IAuditRepository auditRepository)
            : base(uowProvider, logger, eventMessagesFactory)
        {
            _relationRepository = relationRepository;
            _relationTypeRepository = relationTypeRepository;
            _auditRepository = auditRepository;
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        /// <inheritdoc />
        public IRelation GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.Get(id);
            }
        }

        /// <inheritdoc />
        public IRelationType GetRelationTypeById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.Get(id);
            }
        }

        /// <inheritdoc />
        public IRelationType GetRelationTypeById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.Get(id);
            }
        }

        /// <inheritdoc />
        public IRelationType GetRelationTypeByAlias(string alias) => GetRelationType(alias);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetAllRelations(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.GetMany(ids);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetAllRelationsByRelationType(IRelationType relationType)
        {
            return GetAllRelationsByRelationType(relationType.Id);
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetAllRelationsByRelationType(int relationTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
                return _relationRepository.Get(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationTypeRepository.GetMany(ids);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByParentId(int id) => GetByParentId(id, null);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByParentId(int id, string relationTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                if (relationTypeAlias.IsNullOrWhiteSpace())
                {
                    var qry1 = Query<IRelation>().Where(x => x.ParentId == id);
                    return _relationRepository.Get(qry1);
                }

                var relationType = GetRelationType(relationTypeAlias);
                if (relationType == null)
                    return Enumerable.Empty<IRelation>();

                var qry2 = Query<IRelation>().Where(x => x.ParentId == id && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(qry2);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent) => GetByParentId(parent.Id);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias) => GetByParentId(parent.Id, relationTypeAlias);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByChildId(int id) => GetByChildId(id, null);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByChildId(int id, string relationTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                if (relationTypeAlias.IsNullOrWhiteSpace())
                {
                    var qry1 = Query<IRelation>().Where(x => x.ChildId == id);
                    return _relationRepository.Get(qry1);
                }

                var relationType = GetRelationType(relationTypeAlias);
                if (relationType == null)
                    return Enumerable.Empty<IRelation>();

                var qry2 = Query<IRelation>().Where(x => x.ChildId == id && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(qry2);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByChild(IUmbracoEntity child) => GetByChildId(child.Id);

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias) => GetByChildId(child.Id, relationTypeAlias);

        /// <inheritdoc />
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
                var relationType = GetRelationType(relationTypeAlias);
                if (relationType == null)
                    return Enumerable.Empty<IRelation>();

                var query = Query<IRelation>().Where(x => (x.ChildId == id || x.ParentId == id) && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName)
        {
            List<int> relationTypeIds;
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                //This is a silly query - but i guess it's needed in case someone has more than one relation with the same Name (not alias), odd.
                var query = Query<IRelationType>().Where(x => x.Name == relationTypeName);
                var relationTypes = _relationTypeRepository.Get(query);
                relationTypeIds = relationTypes.Select(x => x.Id).ToList();
            }

            return relationTypeIds.Count == 0
                ? Enumerable.Empty<IRelation>()
                : GetRelationsByListOfTypeIds(relationTypeIds);
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias)
        {
            var relationType = GetRelationType(relationTypeAlias);
            
            return relationType == null
                ? Enumerable.Empty<IRelation>()
                : GetRelationsByListOfTypeIds(new[] { relationType.Id });
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetByRelationTypeId(int relationTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
                return _relationRepository.Get(query);
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRelation> GetPagedByRelationTypeId(int relationTypeId, long pageIndex, int pageSize, out long totalRecords, Ordering ordering = null)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
                return _relationRepository.GetPagedRelationsByQuery(query, pageIndex, pageSize, out totalRecords, ordering);
            }
        }

        /// <inheritdoc />
        public IUmbracoEntity GetChildEntityFromRelation(IRelation relation)
        {
            var objectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
            return _entityService.Get(relation.ChildId, objectType);
        }

        /// <inheritdoc />
        public IUmbracoEntity GetParentEntityFromRelation(IRelation relation)
        {
            var objectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);
            return _entityService.Get(relation.ParentId, objectType);
        }

        /// <inheritdoc />
        public Tuple<IUmbracoEntity, IUmbracoEntity> GetEntitiesFromRelation(IRelation relation)
        {
            var childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
            var parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

            var child = _entityService.Get(relation.ChildId, childObjectType);
            var parent = _entityService.Get(relation.ParentId, parentObjectType);

            return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
        }

        /// <inheritdoc />
        public IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations)
        {
            // Trying to avoid full N+1 lookups, so we'll group by the object type and then use the GetAll
            // method to lookup batches of entities for each parent object type

            foreach (var groupedRelations in relations.GroupBy(x => ObjectTypes.GetUmbracoObjectType(x.ChildObjectType)))
            {
                var objectType = groupedRelations.Key;
                var ids = groupedRelations.Select(x => x.ChildId).ToArray();
                foreach (var e in _entityService.GetAll(objectType, ids))
                    yield return e;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations)
        {
            // Trying to avoid full N+1 lookups, so we'll group by the object type and then use the GetAll
            // method to lookup batches of entities for each parent object type

            foreach (var groupedRelations in relations.GroupBy(x => ObjectTypes.GetUmbracoObjectType(x.ParentObjectType)))
            {
                var objectType = groupedRelations.Key;
                var ids = groupedRelations.Select(x => x.ParentId).ToArray();
                foreach (var e in _entityService.GetAll(objectType, ids))
                    yield return e;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.GetPagedParentEntitiesByChildId(id, pageIndex, pageSize, out totalChildren, entityTypes.Select(x => x.GetGuid()).ToArray());
            }
        }

        /// <inheritdoc />
        public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _relationRepository.GetPagedChildEntitiesByParentId(id, pageIndex, pageSize, out totalChildren, entityTypes.Select(x => x.GetGuid()).ToArray());
            }
        }

        /// <inheritdoc />
        public IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations)
        {
            //TODO: Argh! N+1 

            foreach (var relation in relations)
            {
                var childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
                var parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

                var child = _entityService.Get(relation.ChildId, childObjectType);
                var parent = _entityService.Get(relation.ParentId, parentObjectType);

                yield return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
            }
        }

        /// <inheritdoc />
        public IRelation Relate(int parentId, int childId, IRelationType relationType)
        {
            // Ensure that the RelationType has an identity before using it to relate two entities
            if (relationType.HasIdentity == false)
                Save(relationType);

            //TODO: We don't check if this exists first, it will throw some sort of data integrity exception if it already exists, is that ok?

            var relation = new Relation(parentId, childId, relationType);

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IRelation>(relation);
                if (scope.Events.DispatchCancelable(SavingRelation, this, saveEventArgs))
                {
                    scope.Complete();
                    return relation; // TODO: returning sth that does not exist here?!
                }

                _relationRepository.Save(relation);
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelation, this, saveEventArgs);
                scope.Complete();
                return relation;
            }
        }

        /// <inheritdoc />
        public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType)
        {
            return Relate(parent.Id, child.Id, relationType);
        }

        /// <inheritdoc />
        public IRelation Relate(int parentId, int childId, string relationTypeAlias)
        {
            var relationType = GetRelationTypeByAlias(relationTypeAlias);
            if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
                throw new ArgumentNullException(string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));

            return Relate(parentId, childId, relationType);
        }

        /// <inheritdoc />
        public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias)
        {
            var relationType = GetRelationTypeByAlias(relationTypeAlias);
            if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
                throw new ArgumentNullException(string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));

            return Relate(parent.Id, child.Id, relationType);
        }

        /// <inheritdoc />
        public bool HasRelations(IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <inheritdoc />
        public bool IsRelated(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == id || x.ChildId == id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <inheritdoc />
        public bool AreRelated(int parentId, int childId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == parentId && x.ChildId == childId);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <inheritdoc />
        public bool AreRelated(int parentId, int childId, string relationTypeAlias)
        {
            var relType = GetRelationTypeByAlias(relationTypeAlias);
            if (relType == null)
                return false;

            return AreRelated(parentId, childId, relType);
        }


        /// <inheritdoc />
        public bool AreRelated(int parentId, int childId, IRelationType relationType)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelation>().Where(x => x.ParentId == parentId && x.ChildId == childId && x.RelationTypeId == relationType.Id);
                return _relationRepository.Get(query).Any();
            }
        }

        /// <inheritdoc />
        public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child)
        {
            return AreRelated(parent.Id, child.Id);
        }

        /// <inheritdoc />
        public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias)
        {
            return AreRelated(parent.Id, child.Id, relationTypeAlias);
        }


        /// <inheritdoc />
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

        public void Save(IEnumerable<IRelation> relations)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IRelation>(relations);
                if (scope.Events.DispatchCancelable(SavingRelation, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _relationRepository.Save(relations);
                scope.Complete();
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelation, this, saveEventArgs);
            }
        }

        /// <inheritdoc />
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
                Audit(AuditType.Save, Constants.Security.SuperUserId, relationType.Id, $"Saved relation type: {relationType.Name}");
                scope.Complete();
                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(SavedRelationType, this, saveEventArgs);
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void DeleteRelationsOfType(IRelationType relationType)
        {
            var relations = new List<IRelation>();
            using (var scope = ScopeProvider.CreateScope())
            {
                var query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
                relations.AddRange(_relationRepository.Get(query).ToList());

                //TODO: N+1, we should be able to do this in a single call

                foreach (var relation in relations)
                    _relationRepository.Delete(relation);

                scope.Complete();

                scope.Events.Dispatch(DeletedRelation, this, new DeleteEventArgs<IRelation>(relations, false));
            }
        }

        #region Private Methods

        private IRelationType GetRelationType(string relationTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IRelationType>().Where(x => x.Alias == relationTypeAlias);
                return _relationTypeRepository.Get(query).FirstOrDefault();
            }
        }

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

        private void Audit(AuditType type, int userId, int objectId, string message = null)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.RelationType), message));
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
