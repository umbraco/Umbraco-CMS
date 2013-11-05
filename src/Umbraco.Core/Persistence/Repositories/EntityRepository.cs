using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Strings;

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

        public IUmbracoEntity GetByKey(Guid key)
        {
            var sql = GetBaseWhere(GetBase, false, false, key);
            var nodeDto = _work.Database.FirstOrDefault<UmbracoEntityDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);

            return entity;
        }

        public IUmbracoEntity GetByKey(Guid key, Guid objectTypeId)
        {
            bool isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            bool isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var sql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, key).Append(GetGroupBy(isContent, isMedia));
            var nodeDto = _work.Database.FirstOrDefault<UmbracoEntityDto>(sql);
            if (nodeDto == null)
                return null;

            var factory = new UmbracoEntityFactory();
            var entity = factory.BuildEntity(nodeDto);

            return entity;
        }

        public virtual IUmbracoEntity Get(int id)
        {
            var sql = GetBaseWhere(GetBase, false, false, id);
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
            bool isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);
            var sql = GetBaseWhere(GetBase, isContent, isMedia, objectTypeId, id).Append(GetGroupBy(isContent, isMedia));
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
                bool isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);
                var sql = GetBaseWhere(GetBase, isContent, isMedia, string.Empty, objectTypeId).Append(GetGroupBy(isContent, isMedia));
                var dtos = isMedia
                               ? _work.Database.Fetch<UmbracoEntityDto, UmbracoPropertyDto, UmbracoEntityDto>(
                                   new UmbracoEntityRelator().Map, sql)
                               : _work.Database.Fetch<UmbracoEntityDto>(sql);
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
            var wheres = string.Concat(" AND ", string.Join(" AND ", ((Query<IUmbracoEntity>) query).WhereClauses()));
            var sqlClause = GetBase(false, false, wheres);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy(false, false));

            var dtos = _work.Database.Fetch<UmbracoEntityDto>(sql);

            var factory = new UmbracoEntityFactory();
            var list = dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToList();

            return list;
        }

        public virtual IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectTypeId)
        {
            bool isContent = objectTypeId == new Guid(Constants.ObjectTypes.Document);
            bool isMedia = objectTypeId == new Guid(Constants.ObjectTypes.Media);

            var wheres = string.Concat(" AND ", string.Join(" AND ", ((Query<IUmbracoEntity>)query).WhereClauses()));
            var sqlClause = GetBaseWhere(GetBase, isContent, isMedia, wheres, objectTypeId);
            var translator = new SqlTranslator<IUmbracoEntity>(sqlClause, query);
            var sql = translator.Translate().Append(GetGroupBy(isContent, isMedia));

            var factory = new UmbracoEntityFactory();

            if (isMedia)
            {
                //treat media differently for now
                var dtos = _work.Database.Fetch<UmbracoEntityDto, UmbracoPropertyDto, UmbracoEntityDto>(
                    new UmbracoEntityRelator().Map, sql);
                return dtos.Select(factory.BuildEntity).Cast<IUmbracoEntity>().ToArray();
            }
            else
            {
                //use dynamic so that we can get ALL properties from the SQL
                //we'll have to stitch stuff together manually but we can get our
                //additional data to put in the dictionary.
                var dtos = _work.Database.Fetch<dynamic>(sql);
                var entityProps = TypeHelper.GetPublicProperties(typeof (IUmbracoEntity)).Select(x => x.Name).ToArray();
                var result = new List<IUmbracoEntity>();
                foreach (var d in dtos)
                {
                    //build the initial entity
                    IUmbracoEntity entity = factory.BuildEntityFromDynamic(d);

                    //convert the dynamic row to dictionary
                    var asDictionary = (IDictionary<string, object>) d;
                    
                    //figure out what extra properties we have that are not on the IUmbracoEntity and add them to additional data
                    foreach (var k in asDictionary.Keys
                        .Select(x => new {orig = x, title = x.ConvertCase(StringAliasCaseType.PascalCase)})
                        .Where(x => entityProps.InvariantContains(x.title) == false))
                    {
                        entity.AdditionalData[k.title] = asDictionary[k.orig];
                    }

                    result.Add(entity);
                }
                return result;
            }
        }

        #endregion

        #region Sql Statements

        protected virtual Sql GetBase(bool isContent, bool isMedia, string additionWhereStatement = "")
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

            if (isContent || isMedia)
            {
                columns.Add("published.versionId as publishedVersion");
                columns.Add("latest.versionId as newestVersion");
                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("contenttype.isContainer");
            }

            if (isMedia)
            {
                columns.Add("property.dataNvarchar as umbracoFile");
                columns.Add("property.propertyEditorAlias");
            }

            var sql = new Sql()
                .Select(columns.ToArray())
                .From("umbracoNode umbracoNode")
                .LeftJoin("umbracoNode parent").On("parent.parentID = umbracoNode.id");


            if (isContent || isMedia)
            {
                sql.InnerJoin("cmsContent content").On("content.nodeId = umbracoNode.id")
                   .LeftJoin("cmsContentType contenttype").On("contenttype.nodeId = content.contentType")
                   .LeftJoin(
                       "(SELECT nodeId, versionId FROM cmsDocument WHERE published = 1 GROUP BY nodeId, versionId) as published")
                   .On("umbracoNode.id = published.nodeId")
                   .LeftJoin(
                       "(SELECT nodeId, versionId FROM cmsDocument WHERE newest = 1 GROUP BY nodeId, versionId) as latest")
                   .On("umbracoNode.id = latest.nodeId");
            }

            if (isMedia)
            {
                sql.LeftJoin(
                    "(SELECT contentNodeId, versionId, dataNvarchar, propertyEditorAlias FROM cmsPropertyData " +
                    "INNER JOIN umbracoNode ON cmsPropertyData.contentNodeId = umbracoNode.id " +
                    "INNER JOIN cmsPropertyType ON cmsPropertyType.id = cmsPropertyData.propertytypeid " +
                    "INNER JOIN cmsDataType ON cmsPropertyType.dataTypeId = cmsDataType.nodeId "+
                    "WHERE umbracoNode.nodeObjectType = '" + Constants.ObjectTypes.Media + "'" + additionWhereStatement + ") as property")
                   .On("umbracoNode.id = property.contentNodeId");
            }

            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, string, Sql> baseQuery, bool isContent, bool isMedia, string additionWhereStatement, Guid nodeObjectType)
        {
            var sql = baseQuery(isContent, isMedia, additionWhereStatement)
                .Where("umbracoNode.nodeObjectType = @NodeObjectType", new { NodeObjectType = nodeObjectType });
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, string, Sql> baseQuery, bool isContent, bool isMedia, int id)
        {
            var sql = baseQuery(isContent, isMedia, " AND umbracoNode.id = '"+ id +"'")
                .Where("umbracoNode.id = @Id", new { Id = id })
                .Append(GetGroupBy(isContent, isMedia));
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, string, Sql> baseQuery, bool isContent, bool isMedia, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, " AND umbracoNode.uniqueID = '" + key + "'")
                .Where("umbracoNode.uniqueID = @UniqueID", new { UniqueID = key })
                .Append(GetGroupBy(isContent, isMedia));
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, string, Sql> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, int id)
        {
            var sql = baseQuery(isContent, isMedia, " AND umbracoNode.id = '"+ id +"'")
                .Where("umbracoNode.id = @Id AND umbracoNode.nodeObjectType = @NodeObjectType",
                       new {Id = id, NodeObjectType = nodeObjectType});
            return sql;
        }

        protected virtual Sql GetBaseWhere(Func<bool, bool, string, Sql> baseQuery, bool isContent, bool isMedia, Guid nodeObjectType, Guid key)
        {
            var sql = baseQuery(isContent, isMedia, " AND umbracoNode.uniqueID = '" + key + "'")
                .Where("umbracoNode.uniqueID = @UniqueID AND umbracoNode.nodeObjectType = @NodeObjectType",
                       new { UniqueID = key, NodeObjectType = nodeObjectType });
            return sql;
        }

        protected virtual Sql GetGroupBy(bool isContent, bool isMedia)
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

            if (isContent || isMedia)
            {
                columns.Add("published.versionId");
                columns.Add("latest.versionId");
                columns.Add("contenttype.alias");
                columns.Add("contenttype.icon");
                columns.Add("contenttype.thumbnail");
                columns.Add("contenttype.isContainer");
            }

            if (isMedia)
            {
                columns.Add("property.dataNvarchar");
                columns.Add("property.propertyEditorAlias");
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

            [Column("publishedVersion")]
            public Guid PublishedVersion { get; set; }

            [Column("newestVersion")]
            public Guid NewestVersion { get; set; }

            [Column("alias")]
            public string Alias { get; set; }

            [Column("icon")]
            public string Icon { get; set; }

            [Column("thumbnail")]
            public string Thumbnail { get; set; }

            [ResultColumn]
            public List<UmbracoPropertyDto> UmbracoPropertyDtos { get; set; }
        }

        [ExplicitColumns]
        internal class UmbracoPropertyDto
        {
            [Column("propertyEditorAlias")]
            public string PropertyEditorAlias { get; set; }

            [Column("umbracoFile")]
            public string UmbracoFile { get; set; }
        }

        internal class UmbracoEntityRelator
        {
            internal UmbracoEntityDto Current;

            internal UmbracoEntityDto Map(UmbracoEntityDto a, UmbracoPropertyDto p)
            {
                // Terminating call.  Since we can return null from this function
                // we need to be ready for PetaPoco to callback later with null
                // parameters
                if (a == null)
                    return Current;

                // Is this the same UmbracoEntityDto as the current one we're processing
                if (Current != null && Current.UniqueId == a.UniqueId)
                {
                    // Yes, just add this UmbracoPropertyDto to the current UmbracoEntityDto's collection
                    Current.UmbracoPropertyDtos.Add(p);

                    // Return null to indicate we're not done with this UmbracoEntityDto yet
                    return null;
                }

                // This is a different UmbracoEntityDto to the current one, or this is the 
                // first time through and we don't have a Tab yet

                // Save the current UmbracoEntityDto
                var prev = Current;

                // Setup the new current UmbracoEntityDto
                Current = a;
                Current.UmbracoPropertyDtos = new List<UmbracoPropertyDto>();
                Current.UmbracoPropertyDtos.Add(p);

                // Return the now populated previous UmbracoEntityDto (or null if first time through)
                return prev;
            }
        }
        #endregion
    }
}