using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class VersionableRepositoryBase<TId, TEntity> : PetaPocoRepositoryBase<TId, TEntity>
        where TEntity : class, IAggregateRoot
    {
        protected VersionableRepositoryBase(IDatabaseUnitOfWork work) : base(work)
        {
        }

        protected VersionableRepositoryBase(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region IRepositoryVersionable Implementation

        public virtual IEnumerable<TEntity> GetAllVersions(int id)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId)
                .Where<NodeDto>(x => x.NodeId == id)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql);
            foreach (var dto in dtos)
            {
                yield return GetByVersion(dto.VersionId);
            }
        }

        public virtual void DeleteVersion(Guid versionId)
        {
            var dto = Database.FirstOrDefault<ContentVersionDto>("WHERE versionId = @VersionId", new { VersionId = versionId });
            if(dto == null) return;

            //Ensure that the lastest version is not deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = dto.NodeId });
            if(latestVersionDto.VersionId == dto.VersionId)
                return;

            using (var transaction = Database.GetTransaction())
            {
                PerformDeleteVersion(dto.NodeId, versionId);

                transaction.Complete();
            }
        }

        public virtual void DeleteVersions(int id, DateTime versionDate)
        {
            //Ensure that the latest version is not part of the versions being deleted
            var latestVersionDto = Database.FirstOrDefault<ContentVersionDto>("WHERE ContentId = @Id ORDER BY VersionDate DESC", new { Id = id });
            var list =
                Database.Fetch<ContentVersionDto>(
                    "WHERE versionId <> @VersionId AND (ContentId = @Id AND VersionDate < @VersionDate)",
                    new {VersionId = latestVersionDto.VersionId, Id = id, VersionDate = versionDate});
            if (list.Any() == false) return;

            using (var transaction = Database.GetTransaction())
            {
                foreach (var dto in list)
                {
                    PerformDeleteVersion(id, dto.VersionId);
                }

                transaction.Complete();
            }
        }

        public abstract TEntity GetByVersion(Guid versionId);

        /// <summary>
        /// Protected method to execute the delete statements for removing a single version for a TEntity item.
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> to delete a version from</param>
        /// <param name="versionId">Guid id of the version to delete</param>
        protected abstract void PerformDeleteVersion(int id, Guid versionId);

        #endregion

        /// <summary>
        /// Updates the tag repository with any tag enabled properties and their values
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tagRepo"></param>
        protected void UpdatePropertyTags(IContentBase entity, ITagsRepository tagRepo)
        {
            foreach (var tagProp in entity.Properties.Where(x => x.TagSupport.Enable))
            {
                if (tagProp.TagSupport.Behavior == PropertyTagBehavior.Remove)
                {
                    //remove the specific tags
                    tagRepo.RemoveTagsFromProperty(
                        entity.Id,
                        tagProp.PropertyTypeId,
                        tagProp.TagSupport.Tags.Select(x => new Tag { Text = x.Item1, Group = x.Item2 }));
                }
                else
                {
                    //assign the tags
                    tagRepo.AssignTagsToProperty(
                        entity.Id,
                        tagProp.PropertyTypeId,
                        tagProp.TagSupport.Tags.Select(x => new Tag { Text = x.Item1, Group = x.Item2 }),
                        tagProp.TagSupport.Behavior == PropertyTagBehavior.Replace);
                }
            }
        }
        
        /// <summary>
        /// This is a fix for U4-1407 - when property types are added to a content type - the property of the entity are not actually created
        /// and we get YSODs
        /// </summary>
        /// <param name="id"></param>
        /// <param name="versionId"></param>
        /// <param name="contentType"></param>
        /// <param name="createDate"></param>
        /// <param name="updateDate"></param>
        /// <returns></returns>
        protected PropertyCollection GetPropertyCollection(int id, Guid versionId, IContentTypeComposition contentType, DateTime createDate, DateTime updateDate)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Where<PropertyDataDto>(x => x.NodeId == id)
                .Where<PropertyDataDto>(x => x.VersionId == versionId);

            var propertyDataDtos = Database.Fetch<PropertyDataDto, PropertyTypeDto>(sql);
            var propertyFactory = new PropertyFactory(contentType, versionId, id, createDate, updateDate);
            var properties = propertyFactory.BuildEntity(propertyDataDtos).ToArray();

            var newProperties = properties.Where(x => x.HasIdentity == false && x.PropertyType.HasIdentity);
            foreach (var property in newProperties)
            {
                var propertyDataDto = new PropertyDataDto { NodeId = id, PropertyTypeId = property.PropertyTypeId, VersionId = versionId };
                int primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));

                property.Version = versionId;
                property.Id = primaryKey;
            }

            return new PropertyCollection(properties);
        }
    }
}