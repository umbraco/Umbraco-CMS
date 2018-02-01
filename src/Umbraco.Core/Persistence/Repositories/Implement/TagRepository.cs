using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class TagRepository : NPocoRepositoryBase<int, ITag>, ITagRepository
    {
        public TagRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        #region Manage Tag Entities

        /// <inheritdoc />
        protected override Guid NodeObjectTypeId => throw new NotSupportedException();

        /// <inheritdoc />
        protected override ITag PerformGet(int id)
        {
            var sql = Sql().Select<TagDto>().From<TagDto>().Where<TagDto>(x => x.Id == id);
            var dto = Database.Fetch<TagDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            return dto == null ? null : TagFactory.BuildEntity(dto);
        }

        /// <inheritdoc />
        protected override IEnumerable<ITag> PerformGetAll(params int[] ids)
        {
            var dtos = ids.Length == 0
                ? Database.Fetch<TagDto>(Sql().Select<TagDto>().From<TagDto>())
                : Database.FetchByGroups<TagDto, int>(ids, 2000, batch => Sql().Select<TagDto>().From<TagDto>().WhereIn<TagDto>(x => x.Id, batch));

            return dtos.Select(TagFactory.BuildEntity).ToList();
        }

        /// <inheritdoc />
        protected override IEnumerable<ITag> PerformGetByQuery(IQuery<ITag> query)
        {
            var sql = Sql().Select<TagDto>().From<TagDto>();
            var translator = new SqlTranslator<ITag>(sql, query);
            sql = translator.Translate();

            return Database.Fetch<TagDto>(sql).Select(TagFactory.BuildEntity).ToList();
        }

        /// <inheritdoc />
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return isCount ? Sql().SelectCount().From<TagDto>() : GetBaseQuery();
        }

        private Sql<ISqlContext> GetBaseQuery()
        {
            return Sql().Select<TagDto>().From<TagDto>();
        }

        /// <inheritdoc />
        protected override string GetBaseWhereClause()
        {
            return "id = @id";
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsTagRelationship WHERE tagId = @id",
                    "DELETE FROM cmsTags WHERE id = @id"
                };
            return list;
        }

        /// <inheritdoc />
        protected override void PersistNewItem(ITag entity)
        {
            ((EntityBase)entity).AddingEntity();

            var dto = TagFactory.BuildDto(entity);
            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        /// <inheritdoc />
        protected override void PersistUpdatedItem(ITag entity)
        {
            ((EntityBase)entity).UpdatingEntity();

            var dto = TagFactory.BuildDto(entity);
            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        #endregion

        #region Assign and Remove Tags

        /// <inheritdoc />
        public void Assign(int contentId, int propertyTypeId, IEnumerable<ITag> tags, bool replaceTags)
        {
            // to no-duplicates array
            var tagsA = tags.Distinct(new TagComparer()).ToArray();

            // no tags?
            if (tagsA.Length == 0)
            {
                // replacing = clear all
                if (replaceTags)
                {
                    var sql0 = Sql().Delete<TagRelationshipDto>().Where<TagRelationshipDto>(x => x.NodeId == contentId && x.PropertyTypeId == propertyTypeId);
                    Database.Execute(sql0);
                }

                // nothing else to do
                return;
            }

            // tags
            // using some clever logic (?) to insert tags that don't exist in 1 query

            var tagSetSql = GetTagSet(tagsA);
            var group = SqlSyntax.GetQuotedColumnName("group");

            // insert tags
            var sql1 = $@"INSERT INTO cmsTags (tag, {group})
SELECT tagSet.tag, tagSet.{group}
FROM {tagSetSql}
LEFT OUTER JOIN cmsTags ON (tagSet.tag = cmsTags.tag AND tagSet.{group} = cmsTags.{group})
WHERE cmsTags.id IS NULL";

            Database.Execute(sql1);

            // if replacing, remove everything first
            if (replaceTags)
            {
                var sql2 = Sql().Delete<TagRelationshipDto>().Where<TagRelationshipDto>(x => x.NodeId == contentId && x.PropertyTypeId == propertyTypeId);
                Database.Execute(sql2);
            }

            // insert relations
            var sql3 = $@"INSERT INTO cmsTagRelationship (nodeId, propertyTypeId, tagId)
SELECT {contentId}, {propertyTypeId}, tagSet2.Id
FROM (
    SELECT t.Id
    FROM {tagSetSql}
    INNER JOIN cmsTags as t ON (tagSet.tag = t.tag AND tagSet.{group} = t.{group})
) AS tagSet2
LEFT OUTER JOIN cmsTagRelationship r ON (tagSet2.id = r.tagId AND r.nodeId = {contentId} AND r.propertyTypeID = {propertyTypeId})
WHERE r.tagId IS NULL";

            Database.Execute(sql3);
        }

        /// <inheritdoc />
        public void Remove(int contentId, int propertyTypeId, IEnumerable<ITag> tags)
        {
            var tagSetSql = GetTagSet(tags);

            var deleteSql = string.Concat("DELETE FROM cmsTagRelationship WHERE nodeId = ",
                                          contentId,
                                          " AND propertyTypeId = ",
                                          propertyTypeId,
                                          " AND tagId IN ",
                                          "(SELECT id FROM cmsTags INNER JOIN ",
                                          tagSetSql,
                                          " ON (TagSet.Tag = cmsTags.Tag and TagSet." + SqlSyntax.GetQuotedColumnName("group") + @" = cmsTags." + SqlSyntax.GetQuotedColumnName("group") + @"))");

            Database.Execute(deleteSql);
        }

        /// <inheritdoc />
        public void RemoveAll(int contentId, int propertyTypeId)
        {
            Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId AND propertyTypeId = @propertyTypeId",
                new { nodeId = contentId, propertyTypeId = propertyTypeId });
        }

        /// <inheritdoc />
        public void RemoveAll(int contentId)
        {
            Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId",
                new { nodeId = contentId });
        }

        // this is a clever way to produce an SQL statement like this:
        //
        // (
        //   SELECT 'Spacesdd' AS Tag, 'default' AS [group]
        //   UNION
        //   SELECT 'Cool' AS tag, 'default' AS [group]
        // ) AS tagSet
        //
        // which we can then use to reduce queries
        //
        private string GetTagSet(IEnumerable<ITag> tags)
        {
            string EscapeSqlString(string s)
            {
                // why were we escaping @ symbols?
                //return NPocoDatabaseExtensions.EscapeAtSymbols(s.Replace("'", "''"));
                return s.Replace("'", "''");
            }

            var sql = new StringBuilder();
            var group = SqlSyntax.GetQuotedColumnName("group");
            var first = true;

            sql.Append("(");

            foreach (var tag in tags)
            {
                if (first) first = false;
                else sql.Append(" UNION ");

                sql.Append("SELECT N'");
                sql.Append(EscapeSqlString(tag.Text));
                sql.Append("' AS tag, '");
                sql.Append(EscapeSqlString(tag.Group));
                sql.Append("' AS ");
                sql.Append(group);
            }

            sql.Append(") AS tagSet");

            return sql.ToString();
        }

        // used to run Distinct() on tags
        private class TagComparer : IEqualityComparer<ITag>
        {
            public bool Equals(ITag x, ITag y)
            {
                return ReferenceEquals(x, y) // takes care of both being null
                       || x != null && y != null && x.Text == y.Text && x.Group == y.Group;
            }

            public int GetHashCode(ITag obj)
            {
                unchecked
                {
                    return (obj.Text.GetHashCode() * 397) ^ obj.Group.GetHashCode();
                }
            }
        }

        #endregion

        #region Queries

        // TODO
        // consider caching implications
        // add lookups for parentId or path (ie get content in tag group, that are descendants of x)

        public TaggedEntity GetTaggedEntityByKey(Guid key)
        {
            var sql = Sql()
                .Select("cmsTagRelationship.nodeId, cmsPropertyType.Alias, cmsPropertyType.id as propertyTypeId, cmsTags.tag, cmsTags.id as tagId, cmsTags." + SqlSyntax.GetQuotedColumnName("group"))
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.UniqueId == key);

            return CreateTaggedEntityCollection(Database.Fetch<dynamic>(sql)).FirstOrDefault();
        }

        public TaggedEntity GetTaggedEntityById(int id)
        {
            var sql = Sql()
                .Select("cmsTagRelationship.nodeId, cmsPropertyType.Alias, cmsPropertyType.id as propertyTypeId, cmsTags.tag, cmsTags.id as tagId, cmsTags." + SqlSyntax.GetQuotedColumnName("group"))
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeId == id);

            return CreateTaggedEntityCollection(Database.Fetch<dynamic>(sql)).FirstOrDefault();
        }

        public IEnumerable<TaggedEntity> GetTaggedEntitiesByTagGroup(TaggableObjectTypes objectType, string tagGroup)
        {
            var sql = Sql()
                .Select("cmsTagRelationship.nodeId, cmsPropertyType.Alias, cmsPropertyType.id as propertyTypeId, cmsTags.tag, cmsTags.id as tagId, cmsTags." + SqlSyntax.GetQuotedColumnName("group"))
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<TagDto>(dto => dto.Group == tagGroup);

            if (objectType != TaggableObjectTypes.All)
            {
                var nodeObjectType = GetNodeObjectType(objectType);
                sql = sql
                    .Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);
            }

            return CreateTaggedEntityCollection(
                Database.Fetch<dynamic>(sql));
        }

        public IEnumerable<TaggedEntity> GetTaggedEntitiesByTag(TaggableObjectTypes objectType, string tag, string tagGroup = null)
        {
            var sql = Sql()
                .Select("cmsTagRelationship.nodeId, cmsPropertyType.Alias, cmsPropertyType.id as propertyTypeId, cmsTags.tag, cmsTags.id as tagId, cmsTags." + SqlSyntax.GetQuotedColumnName("group"))
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<TagDto>(dto => dto.Text == tag);

            if (objectType != TaggableObjectTypes.All)
            {
                var nodeObjectType = GetNodeObjectType(objectType);
                sql = sql
                    .Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);
            }

            if (tagGroup.IsNullOrWhiteSpace() == false)
            {
                sql = sql.Where<TagDto>(dto => dto.Group == tagGroup);
            }

            return CreateTaggedEntityCollection(
                Database.Fetch<dynamic>(sql));
        }

        private IEnumerable<TaggedEntity> CreateTaggedEntityCollection(IEnumerable<dynamic> dbResult)
        {
            foreach (var node in dbResult.GroupBy(x => (int)x.nodeId))
            {
                var properties = new List<TaggedProperty>();
                foreach (var propertyType in node.GroupBy(x => new { id = (int)x.propertyTypeId, alias = (string)x.Alias }))
                {
                    var tags = propertyType.Select(x => new Tag((int)x.tagId, (string)x.group, (string)x.tag));
                    properties.Add(new TaggedProperty(propertyType.Key.id, propertyType.Key.alias, tags));
                }
                yield return new TaggedEntity(node.Key, properties);
            }
        }

        public IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string group = null)
        {
            var sql = GetTagsQuerySelect(true);

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            if (objectType != TaggableObjectTypes.All)
            {
                var nodeObjectType = GetNodeObjectType(objectType);
                sql = sql
                    .Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);
            }

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            sql = ApplyGroupByToTagsQuery(sql);

            return ExecuteTagsQuery(sql);
        }

        public IEnumerable<ITag> GetTagsForEntity(int contentId, string group = null)
        {
            var sql = GetTagsQuerySelect();

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            sql = sql
                .Where<NodeDto>(dto => dto.NodeId == contentId);

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            return ExecuteTagsQuery(sql);
        }

        public IEnumerable<ITag> GetTagsForEntity(Guid contentId, string group = null)
        {
            var sql = GetTagsQuerySelect();

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            sql = sql
                .Where<NodeDto>(dto => dto.UniqueId == contentId);

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            return ExecuteTagsQuery(sql);
        }

        public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string group = null)
        {
            var sql = GetTagsQuerySelect();

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            sql = sql
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .Where<NodeDto>(dto => dto.NodeId == contentId)
                .Where<PropertyTypeDto>(dto => dto.Alias == propertyTypeAlias);

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            return ExecuteTagsQuery(sql);
        }

        public IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string group = null)
        {
            var sql = GetTagsQuerySelect();

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            sql = sql
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .Where<NodeDto>(dto => dto.UniqueId == contentId)
                .Where<PropertyTypeDto>(dto => dto.Alias == propertyTypeAlias);

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            return ExecuteTagsQuery(sql);
        }

        private Sql<ISqlContext> GetTagsQuerySelect(bool withGrouping = false)
        {
            var sql = Sql();

            if (withGrouping)
            {
                sql = sql.Select("cmsTags.id, cmsTags.tag, cmsTags." + SqlSyntax.GetQuotedColumnName("group") + @", Count(*) NodeCount");
            }
            else
            {
                sql = sql.Select("DISTINCT cmsTags.*");
            }

            return sql;
        }

        private Sql<ISqlContext> ApplyRelationshipJoinToTagsQuery(Sql<ISqlContext> sql)
        {
            return sql
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId);
        }

        private Sql<ISqlContext> ApplyGroupFilterToTagsQuery(Sql<ISqlContext> sql, string group)
        {
            if (group.IsNullOrWhiteSpace() == false)
            {
                sql = sql.Where<TagDto>(dto => dto.Group == group);
            }

            return sql;
        }

        private Sql<ISqlContext> ApplyGroupByToTagsQuery(Sql<ISqlContext> sql)
        {
            return sql.GroupBy("cmsTags.id", "cmsTags.tag", "cmsTags." + SqlSyntax.GetQuotedColumnName("group") + @"");
        }

        private IEnumerable<ITag> ExecuteTagsQuery(Sql sql)
        {
            return Database.Fetch<TagDto>(sql).Select(TagFactory.BuildEntity);
        }

        private Guid GetNodeObjectType(TaggableObjectTypes type)
        {
            switch (type)
            {
                case TaggableObjectTypes.Content:
                    return Constants.ObjectTypes.Document;
                case TaggableObjectTypes.Media:
                    return Constants.ObjectTypes.Media;
                case TaggableObjectTypes.Member:
                    return Constants.ObjectTypes.Member;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        #endregion
    }
}
