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
            bool isContentOrMedia = objectTypeId == new Guid(Constants.ObjectTypes.Document) || objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var sql = GetBaseWhere(GetBase, isContentOrMedia, objectTypeId, id).Append(GetGroupBy(isContentOrMedia));
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
                bool isContentOrMedia = objectTypeId == new Guid(Constants.ObjectTypes.Document) || objectTypeId == new Guid(Constants.ObjectTypes.Media);
                var sql = GetBaseWhere(GetBase, isContentOrMedia, objectTypeId).Append(GetGroupBy(isContentOrMedia));
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
            var sql = translator.Translate().Append(GetGroupBy(false));

            var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            bool isContentOrMedia = objectTypeId == new Guid(Constants.ObjectTypes.Document) || objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var sqlClause = GetBaseWhere(GetBase, isContentOrMedia, objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy(isContentOrMedia));

            var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        #endregion

        #region Sql Statements

        protected virtual Sql GetBase(bool isContentOrMedia)
        {
            var columns = new List<object>
                              {
                                  "umbracoNode.id",
                                  "umbracoNode.trashed",
                                  "umbracoNode.parentID",
                                  "umbracoNode.nodeUser",
                                  "umbracoNode.level",
                                  "umbracoNode.path",
                                  "umbracoNode.sortOrder",
                                  "umbracoNode.uniqueID",
                                  "umbracoNode.text",
                                  "umbracoNode.nodeObjectType",
                                  "umbracoNode.createDate",
                                  "COUNT(parent.parentID) as children"
                              };

            if (isContentOrMedia)
            {
                columns.Add("published.versionId as publishedVerison");
                columns.Add("latest.versionId as newestVersion");
                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("property.dataNvarchar as umbracoFile");
            }

            var sql = new Sql()
                .Select(columns.ToArray())
                .From("umbracoNode umbracoNode")
                .LeftJoin("umbracoNode parent").On("parent.parentID = umbracoNode.id");


            if (isContentOrMedia)
            {
                sql.InnerJoin("cmsContent content").On("content.nodeId = umbracoNode.id")
                   .LeftJoin("cmsContentType contenttype").On("contenttype.nodeId = content.contentType")
                   .LeftJoin(
                       "(SELECT nodeId, versionId FROM cmsDocument WHERE published = 1 GROUP BY nodeId, versionId) as published")
                   .On("umbracoNode.id = published.nodeId")
                   .LeftJoin(
                       "(SELECT nodeId, versionId FROM cmsDocument WHERE newest = 1 GROUP BY nodeId, versionId) as latest")
                   .On("umbracoNode.id = latest.nodeId")
                   .LeftJoin(
                       "(SELECT contentNodeId, dataNvarchar FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyType.id = cmsPropertyData.propertytypeid"+
                       " INNER JOIN cmsDataType ON cmsPropertyType.dataTypeId = cmsDataType.nodeId WHERE cmsDataType.controlId = '"+ Constants.PropertyEditors.UploadField +"') as property")
                   .On("umbracoNode.id = property.contentNodeId");
            }

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContentOrMedia, Guid id)
        {
            var sql = baseQuery(isContentOrMedia)
                .Where("umbracoNode.nodeObjectType = @NodeObjectType", new { NodeObjectType = id });
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContentOrMedia, int id)
        {
            var sql = baseQuery(isContentOrMedia)
                .Where("umbracoNode.id = @Id", new { Id = id })
                .Append(GetGroupBy(isContentOrMedia));
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, Sql> baseQuery, bool isContentOrMedia, Guid objectId, int id)
        {
            var sql = baseQuery(isContentOrMedia)
                .Where("umbracoNode.id = @Id AND umbracoNode.nodeObjectType = @NodeObjectType",
                       new {Id = id, NodeObjectType = objectId});
            return sql;
        }

        protected virtual Sql GetGroupBy(bool isContentOrMedia)
        {
            var columns = new List<object>
                              {
                                  "umbracoNode.id",
                                  "umbracoNode.trashed",
                                  "umbracoNode.parentID",
                                  "umbracoNode.nodeUser",
                                  "umbracoNode.level",
                                  "umbracoNode.path",
                                  "umbracoNode.sortOrder",
                                  "umbracoNode.uniqueID",
                                  "umbracoNode.text",
                                  "umbracoNode.nodeObjectType",
                                  "umbracoNode.createDate"
                              };

            if (isContentOrMedia)
            {
                columns.Add("published.versionId");
                columns.Add("latest.versionId");
                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("property.dataNvarchar");
            }

            var sql = new Sql()
                .GroupBy(columns.ToArray())
                .OrderBy("umbracoNode.sortOrder");
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

            [Column("publishedVerison")]
            public Guid PublishedVersion { get; set; }

            [Column("newestVersion")]
            public Guid NewestVersion { get; set; }

            [Column("alias")]
            public string Alias { get; set; }

            [Column("icon")]
            public string Icon { get; set; }

            [Column("thumbnail")]
            public string Thumbnail { get; set; }

            [Column("umbracoFile")]
            public string UmbracoFile { get; set; }
        }
        #endregion
    }
}