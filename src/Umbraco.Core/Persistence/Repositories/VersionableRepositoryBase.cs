using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class VersionableRepositoryBase<TId, TEntity> : PermissionRepository<TId, TEntity>
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

            using (var transaction = Database.GetTransaction())
            {
                PerformDeleteVersion(dto.NodeId, versionId);

                transaction.Complete();
            }
        }

        public virtual void DeleteVersions(int id, DateTime versionDate)
        {
            var list = Database.Fetch<ContentVersionDto>("WHERE ContentId = @Id AND VersionDate < @VersionDate", new { Id = id, VersionDate = versionDate });
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
    }
}