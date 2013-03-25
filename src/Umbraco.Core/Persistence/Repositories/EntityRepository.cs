using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
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
            var sql = GetBaseWhere(GetBase, false, id);
            var nodeDto = _work.Database.FirstOrDefault<UmbracoEntityDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);

            return entity;
        }

        public virtual IUmbracoEntity Get(int id, Guid objectTypeId)
        {
            bool isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var sql = GetBaseWhere(GetBase, isContent, objectTypeId, id).Append(GetGroupBy());
            var nodeDto = _work.Database.FirstOrDefault<UmbracoEntityDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);

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
                bool isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
                var sql = GetBaseWhere(GetBase, isContent, objectTypeId).Append(GetGroupBy());
                var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

                var factory = new UmbracoEntityFactory();

                foreach (var dto in dtos)
                {
                    var entity = factory.BuildEntity(dto);
                    yield return entity;
                }
            }
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query)
        {
            var sqlClause = GetBase(false);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy());

            var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            bool isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            var sqlClause = GetBaseWhere(GetBase, isContent, objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy());

            var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        #endregion

        #region Sql Statements

        protected virtual Sql GetBase(bool isContent)
        {
            var columns = new List<object>
                              {
                                  "main.id",
                                  "main.trashed",
                                  "main.parentID",
                                  "main.nodeUser",
                                  "main.level",
                                  "main.path",
                                  "main.sortOrder",
                                  "main.uniqueID",
                                  "main.text",
                                  "main.nodeObjectType",
                                  "main.createDate",
                                  "COUNT(parent.parentID) as children",
                                  isContent
                                      ? "SUM(CONVERT(int, document.published)) as published"
                                      : "SUM(0) as published"
                              };

            var sql = new Sql()
                .Select(columns.ToArray())
                .From("FROM umbracoNode main")
                .LeftJoin("umbracoNode parent").On("parent.parentID = main.id");


            //NOTE Should this account for newest = 1 ? Scenarios: unsaved, saved not published, published

            if (isContent)
                sql.LeftJoin("LEFT JOIN cmsDocument document").On("document.nodeId = main.id");

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContent, Guid id)
        {
            var sql = baseQuery(isContent)
                .Where("main.nodeObjectType = @NodeObjectType", new {NodeObjectType = id});
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContent, int id)
        {
            var sql = baseQuery(isContent)
                .Where("main.id = @Id", new {Id = id})
                .Append(GetGroupBy());
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContent, Guid objectId, int id)
        {
            var sql = baseQuery(isContent)
                .Where("main.id = @Id AND main.nodeObjectType = @NodeObjectType",
                       new {Id = id, NodeObjectType = objectId});
            return sql;
        }

        protected virtual Sql GetGroupBy()
        {
            var sql = new Sql()
                .GroupBy("main.id", "main.trashed", "main.parentID", "main.nodeUser", "main.level",
                         "main.path", "main.sortOrder", "main.uniqueID", "main.text",
                         "main.nodeObjectType", "main.createDate")
                .OrderBy("main.sortOrder");
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

        #region umbracoNode POCO - Extends NodeDto
        [TableName("umbracoNode")]
        [PrimaryKey("id")]
        [ExplicitColumns]
        internal class UmbracoEntityDto : NodeDto
        {
            [Column("children")]
            public int Children { get; set; }

            [Column("published")]
            public int? HasPublishedVersion { get; set; }
        }
        #endregion
    }
}