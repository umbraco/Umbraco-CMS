using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class RelationService : RepositoryService, IRelationService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;
    private readonly IRelationRepository _relationRepository;
    private readonly IRelationTypeRepository _relationTypeRepository;

    public RelationService(ICoreScopeProvider uowProvider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IEntityService entityService, IRelationRepository relationRepository, IRelationTypeRepository relationTypeRepository, IAuditRepository auditRepository)
        : base(uowProvider, loggerFactory, eventMessagesFactory)
    {
        _relationRepository = relationRepository;
        _relationTypeRepository = relationTypeRepository;
        _auditRepository = auditRepository;
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    /// <inheritdoc />
    public IRelation? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IRelationType? GetRelationTypeById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationTypeRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IRelationType? GetRelationTypeById(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationTypeRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IRelationType? GetRelationTypeByAlias(string alias) => GetRelationType(alias);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetAllRelations(params int[] ids)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationRepository.GetMany(ids);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetAllRelationsByRelationType(IRelationType relationType) =>
        GetAllRelationsByRelationType(relationType.Id);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetAllRelationsByRelationType(int relationTypeId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
            return _relationRepository.Get(query);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationTypeRepository.GetMany(ids);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByParentId(int id) => GetByParentId(id, null);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByParentId(int id, string? relationTypeAlias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            if (relationTypeAlias.IsNullOrWhiteSpace())
            {
                IQuery<IRelation> qry1 = Query<IRelation>().Where(x => x.ParentId == id);
                return _relationRepository.Get(qry1);
            }

            IRelationType? relationType = GetRelationType(relationTypeAlias!);
            if (relationType == null)
            {
                return Enumerable.Empty<IRelation>();
            }

            IQuery<IRelation> qry2 =
                Query<IRelation>().Where(x => x.ParentId == id && x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(qry2);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent) => GetByParentId(parent.Id);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias) =>
        GetByParentId(parent.Id, relationTypeAlias);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByChildId(int id) => GetByChildId(id, null);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByChildId(int id, string? relationTypeAlias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            if (relationTypeAlias.IsNullOrWhiteSpace())
            {
                IQuery<IRelation> qry1 = Query<IRelation>().Where(x => x.ChildId == id);
                return _relationRepository.Get(qry1);
            }

            IRelationType? relationType = GetRelationType(relationTypeAlias!);
            if (relationType == null)
            {
                return Enumerable.Empty<IRelation>();
            }

            IQuery<IRelation> qry2 =
                Query<IRelation>().Where(x => x.ChildId == id && x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(qry2);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByChild(IUmbracoEntity child) => GetByChildId(child.Id);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias) =>
        GetByChildId(child.Id, relationTypeAlias);

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByParentOrChildId(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.ChildId == id || x.ParentId == id);
            return _relationRepository.Get(query);
        }
    }

    public IEnumerable<IRelation> GetByParentOrChildId(int id, string relationTypeAlias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IRelationType? relationType = GetRelationType(relationTypeAlias);
            if (relationType == null)
            {
                return Enumerable.Empty<IRelation>();
            }

            IQuery<IRelation> query = Query<IRelation>().Where(x =>
                (x.ChildId == id || x.ParentId == id) && x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(query);
        }
    }

    /// <inheritdoc />
    public IRelation? GetByParentAndChildId(int parentId, int childId, IRelationType relationType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.ParentId == parentId &&
                                                                    x.ChildId == childId &&
                                                                    x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(query).FirstOrDefault();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName)
    {
        List<int>? relationTypeIds;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            // This is a silly query - but i guess it's needed in case someone has more than one relation with the same Name (not alias), odd.
            IQuery<IRelationType> query = Query<IRelationType>().Where(x => x.Name == relationTypeName);
            IEnumerable<IRelationType> relationTypes = _relationTypeRepository.Get(query);
            relationTypeIds = relationTypes.Select(x => x.Id).ToList();
        }

        return relationTypeIds.Count == 0
            ? Enumerable.Empty<IRelation>()
            : GetRelationsByListOfTypeIds(relationTypeIds);
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias)
    {
        IRelationType? relationType = GetRelationType(relationTypeAlias);

        return relationType == null
            ? Enumerable.Empty<IRelation>()
            : GetRelationsByListOfTypeIds(new[] { relationType.Id });
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetByRelationTypeId(int relationTypeId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
            return _relationRepository.Get(query);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRelation> GetPagedByRelationTypeId(int relationTypeId, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation>? query = Query<IRelation>().Where(x => x.RelationTypeId == relationTypeId);
            return _relationRepository.GetPagedRelationsByQuery(query, pageIndex, pageSize, out totalRecords, ordering);
        }
    }

    /// <inheritdoc />
    public IUmbracoEntity? GetChildEntityFromRelation(IRelation relation)
    {
        UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
        return _entityService.Get(relation.ChildId, objectType);
    }

    /// <inheritdoc />
    public IUmbracoEntity? GetParentEntityFromRelation(IRelation relation)
    {
        UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);
        return _entityService.Get(relation.ParentId, objectType);
    }

    /// <inheritdoc />
    public Tuple<IUmbracoEntity, IUmbracoEntity>? GetEntitiesFromRelation(IRelation relation)
    {
        UmbracoObjectTypes childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
        UmbracoObjectTypes parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

        IEntitySlim? child = _entityService.Get(relation.ChildId, childObjectType);
        IEntitySlim? parent = _entityService.Get(relation.ParentId, parentObjectType);

        if (parent is null || child is null)
        {
            return null;
        }

        return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        // Trying to avoid full N+1 lookups, so we'll group by the object type and then use the GetAll
        // method to lookup batches of entities for each parent object type
        foreach (IGrouping<UmbracoObjectTypes, IRelation> groupedRelations in relations.GroupBy(x =>
                     ObjectTypes.GetUmbracoObjectType(x.ChildObjectType)))
        {
            UmbracoObjectTypes objectType = groupedRelations.Key;
            var ids = groupedRelations.Select(x => x.ChildId).ToArray();
            foreach (IEntitySlim e in _entityService.GetAll(objectType, ids))
            {
                yield return e;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        // Trying to avoid full N+1 lookups, so we'll group by the object type and then use the GetAll
        // method to lookup batches of entities for each parent object type
        foreach (IGrouping<UmbracoObjectTypes, IRelation> groupedRelations in relations.GroupBy(x =>
                     ObjectTypes.GetUmbracoObjectType(x.ParentObjectType)))
        {
            UmbracoObjectTypes objectType = groupedRelations.Key;
            var ids = groupedRelations.Select(x => x.ParentId).ToArray();
            foreach (IEntitySlim e in _entityService.GetAll(objectType, ids))
            {
                yield return e;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationRepository.GetPagedParentEntitiesByChildId(id, pageIndex, pageSize, out totalChildren, entityTypes.Select(x => x.GetGuid()).ToArray());
        }
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _relationRepository.GetPagedChildEntitiesByParentId(id, pageIndex, pageSize, out totalChildren, entityTypes.Select(x => x.GetGuid()).ToArray());
        }
    }

    /// <inheritdoc />
    public IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        // TODO: Argh! N+1
        foreach (IRelation relation in relations)
        {
            UmbracoObjectTypes childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
            UmbracoObjectTypes parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

            IEntitySlim? child = _entityService.Get(relation.ChildId, childObjectType);
            IEntitySlim? parent = _entityService.Get(relation.ParentId, parentObjectType);

            if (parent is not null && child is not null)
            {
                yield return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
            }
        }
    }

    /// <inheritdoc />
    public IRelation Relate(int parentId, int childId, IRelationType relationType)
    {
        // Ensure that the RelationType has an identity before using it to relate two entities
        if (relationType.HasIdentity == false)
        {
            Save(relationType);
        }

        // TODO: We don't check if this exists first, it will throw some sort of data integrity exception if it already exists, is that ok?
        var relation = new Relation(parentId, childId, relationType);

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new RelationSavingNotification(relation, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return relation; // TODO: returning sth that does not exist here?!
            }

            _relationRepository.Save(relation);
            scope.Notifications.Publish(
                new RelationSavedNotification(relation, eventMessages).WithStateFrom(savingNotification));
            scope.Complete();
            return relation;
        }
    }

    /// <inheritdoc />
    public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType) =>
        Relate(parent.Id, child.Id, relationType);

    /// <inheritdoc />
    public IRelation Relate(int parentId, int childId, string relationTypeAlias)
    {
        IRelationType? relationType = GetRelationTypeByAlias(relationTypeAlias);
        if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
        {
            throw new ArgumentNullException(
                string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));
        }

        return Relate(parentId, childId, relationType);
    }

    /// <inheritdoc />
    public IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias)
    {
        IRelationType? relationType = GetRelationTypeByAlias(relationTypeAlias);
        if (relationType == null || string.IsNullOrEmpty(relationType.Alias))
        {
            throw new ArgumentNullException(
                string.Format("No RelationType with Alias '{0}' exists.", relationTypeAlias));
        }

        return Relate(parent.Id, child.Id, relationType);
    }

    /// <inheritdoc />
    public bool HasRelations(IRelationType relationType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(query).Any();
        }
    }

    /// <inheritdoc />
    public bool IsRelated(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.ParentId == id || x.ChildId == id);
            return _relationRepository.Get(query).Any();
        }
    }

    /// <inheritdoc />
    public bool AreRelated(int parentId, int childId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.ParentId == parentId && x.ChildId == childId);
            return _relationRepository.Get(query).Any();
        }
    }

    /// <inheritdoc />
    public bool AreRelated(int parentId, int childId, string relationTypeAlias)
    {
        IRelationType? relType = GetRelationTypeByAlias(relationTypeAlias);
        if (relType == null)
        {
            return false;
        }

        return AreRelated(parentId, childId, relType);
    }

    /// <inheritdoc />
    public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child) => AreRelated(parent.Id, child.Id);

    /// <inheritdoc />
    public bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias) =>
        AreRelated(parent.Id, child.Id, relationTypeAlias);

    /// <inheritdoc />
    public void Save(IRelation relation)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new RelationSavingNotification(relation, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _relationRepository.Save(relation);
            scope.Complete();
            scope.Notifications.Publish(
                new RelationSavedNotification(relation, eventMessages).WithStateFrom(savingNotification));
        }
    }

    public void Save(IEnumerable<IRelation> relations)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IRelation[] relationsA = relations.ToArray();

            EventMessages messages = EventMessagesFactory.Get();
            var savingNotification = new RelationSavingNotification(relationsA, messages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _relationRepository.Save(relationsA);
            scope.Complete();
            scope.Notifications.Publish(
                new RelationSavedNotification(relationsA, messages).WithStateFrom(savingNotification));
        }
    }

    /// <inheritdoc />
    public void Save(IRelationType relationType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new RelationTypeSavingNotification(relationType, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _relationTypeRepository.Save(relationType);
            Audit(AuditType.Save, Constants.Security.SuperUserId, relationType.Id, $"Saved relation type: {relationType.Name}");
            scope.Complete();
            scope.Notifications.Publish(
                new RelationTypeSavedNotification(relationType, eventMessages).WithStateFrom(savingNotification));
        }
    }

    /// <inheritdoc />
    public void Delete(IRelation relation)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new RelationDeletingNotification(relation, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            _relationRepository.Delete(relation);
            scope.Complete();
            scope.Notifications.Publish(
                new RelationDeletedNotification(relation, eventMessages).WithStateFrom(deletingNotification));
        }
    }

    /// <inheritdoc />
    public void Delete(IRelationType relationType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new RelationTypeDeletingNotification(relationType, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            _relationTypeRepository.Delete(relationType);
            scope.Complete();
            scope.Notifications.Publish(
                new RelationTypeDeletedNotification(relationType, eventMessages).WithStateFrom(deletingNotification));
        }
    }

    /// <inheritdoc />
    public void DeleteRelationsOfType(IRelationType relationType)
    {
        var relations = new List<IRelation>();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x => x.RelationTypeId == relationType.Id);
            var allRelations = _relationRepository.Get(query).ToList();
            relations.AddRange(allRelations);

            // TODO: N+1, we should be able to do this in a single call
            foreach (IRelation relation in relations)
            {
                _relationRepository.Delete(relation);
            }

            scope.Complete();

            scope.Notifications.Publish(new RelationDeletedNotification(relations, EventMessagesFactory.Get()));
        }
    }

    public bool AreRelated(int parentId, int childId, IRelationType relationType)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelation> query = Query<IRelation>().Where(x =>
                x.ParentId == parentId && x.ChildId == childId && x.RelationTypeId == relationType.Id);
            return _relationRepository.Get(query).Any();
        }
    }

    #region Private Methods

    private IRelationType? GetRelationType(string relationTypeAlias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IRelationType> query = Query<IRelationType>().Where(x => x.Alias == relationTypeAlias);
            return _relationTypeRepository.Get(query).FirstOrDefault();
        }
    }

    private IEnumerable<IRelation> GetRelationsByListOfTypeIds(IEnumerable<int> relationTypeIds)
    {
        var relations = new List<IRelation>();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            foreach (var relationTypeId in relationTypeIds)
            {
                var id = relationTypeId;
                IQuery<IRelation> query = Query<IRelation>().Where(x => x.RelationTypeId == id);
                IEnumerable<IRelation> relation = _relationRepository.Get(query);
                relations.AddRange(relation);
            }
        }

        return relations;
    }

    private void Audit(AuditType type, int userId, int objectId, string? message = null) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, UmbracoObjectTypes.RelationType.GetName(), message));

    #endregion
}
