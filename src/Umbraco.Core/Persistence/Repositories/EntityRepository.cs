using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the EntityRepository used to query <see cref="IUmbracoEntity"/> objects.
    /// </summary>
    /// <remarks>
    /// This is limited to objects that are based in the umbracoNode-table.
    /// </remarks>
    internal class EntityRepository : DisposableObject, IEntityRepository
    {
        private readonly IDatabaseUnitOfWork _work;

        public EntityRepository(IDatabaseUnitOfWork work)
		{
		    _work = work;
		}

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected internal IDatabaseUnitOfWork UnitOfWork
        {
            get { return _work; }
        }

        /// <summary>
        /// Internal for testing purposes
        /// </summary>
        internal Guid UnitKey
        {
            get { return (Guid)_work.Key; }
        }

        #region Query Methods

        public virtual IUmbracoEntity Get(int id)
        {
            var sql = GetBaseWhere(id);
            var nodeDto = _work.Database.FirstOrDefault<NodeDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);
            
            //TODO Update HasChildren and IsPublished

            return entity;
        }

        public virtual IUmbracoEntity Get(int id, Guid objectTypeId)
        {
            var sql = GetBaseWhere(objectTypeId, id);
            var nodeDto = _work.Database.FirstOrDefault<NodeDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);

            //TODO Update HasChildren and IsPublished

            return entity;
        }

        public virtual IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id, objectTypeId);
                }
            }
            else
            {
                var sql = GetBaseWhere(objectTypeId);
                var dtos = _work.Database.Fetch<NodeDto>(sql);

                var factory = new UmbracoEntityFactory();

                foreach (var dto in dtos)
                {
                    var entity = factory.BuildEntity(dto);
                    //TODO Update HasChildren and IsPublished properties
                    yield return entity;
                }
            }
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBaseQuery();
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = _work.Database.Fetch<NodeDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            //TODO Update HasChildren and IsPublished properties

            return list;
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            var sqlClause = GetBaseWhere(objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = _work.Database.Fetch<NodeDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            //TODO Update HasChildren and IsPublished properties

            return list;
        }

        #endregion

        #region Sql Statements

        protected virtual Sql GetBaseQuery()
        {
            var sql = new Sql()
                .From<NodeDto>();
            return sql;
        }

        protected virtual Sql GetBaseWhere(Guid id)
        {
            var sql = GetBaseQuery()
                .Where<NodeDto>(x => x.NodeObjectType == id);
            return sql;
        }

        protected virtual Sql GetBaseWhere(int id)
        {
            var sql = GetBaseQuery()
                .Where<NodeDto>(x => x.NodeId == id);
            return sql;
        }

        protected virtual Sql GetBaseWhere(Guid objectId, int id)
        {
            var sql = GetBaseWhere(objectId)
                .Where<NodeDto>(x => x.NodeId == id);
            return sql;
        }

        #endregion

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            UnitOfWork.DisposeIfDisposable();
        }
    }
}