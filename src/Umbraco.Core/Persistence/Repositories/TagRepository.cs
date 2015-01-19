using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class TagRepository : PetaPocoRepositoryBase<int, ITag>, ITagRepository
    {
        internal TagRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override ITag PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var tagDto = Database.Fetch<TagDto>(sql).FirstOrDefault();
            if (tagDto == null)
                return null;

            var factory = new TagFactory();
            var entity = factory.BuildEntity(tagDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<ITag> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("id in (@ids)", new {ids = ids});

                //return PerformGetAllOnIds(ids);
            }

            return ConvertFromDtos(Database.Fetch<TagDto>(sql))
                .ToArray();// we don't want to re-iterate again!
        }

        //private IEnumerable<ITag> PerformGetAllOnIds(params int[] ids)
        //{
        //    //TODO: Fix the n+1 query! This one is particularly bad!!!!!!!!

        //    if (ids.Any() == false) yield break;
        //    foreach (var id in ids)
        //    {
        //        yield return Get(id);
        //    }
        //}

        private IEnumerable<ITag> ConvertFromDtos(IEnumerable<TagDto> dtos)
        {
            var factory = new TagFactory();
            foreach (var entity in dtos.Select(factory.BuildEntity))
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);
                yield return entity;
            }
        }

        protected override IEnumerable<ITag> PerformGetByQuery(IQuery<ITag> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ITag>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<TagDto>(sql);

            return ConvertFromDtos(dtos)
                .ToArray();// we don't want to re-iterate again!

        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<TagDto>();
            }
            else
            {
                return GetBaseQuery();
            }
            return sql;
        }

        private static Sql GetBaseQuery()
        {
            var sql = new Sql();
            sql.Select("*").From<TagDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsTagRelationship WHERE tagId = @Id",
                    "DELETE FROM cmsTags WHERE id = @Id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(ITag entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new TagFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ITag entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new TagFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        //TODO: Consider caching implications.

        //TODO: We need to add lookups for parentId or path! (i.e. get content in tag group that are descendants of x)


        public IEnumerable<TaggedEntity> GetTaggedEntitiesByTagGroup(TaggableObjectTypes objectType, string tagGroup)
        {
            var sql = new Sql()
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
                ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>(sql));
        }

        public IEnumerable<TaggedEntity> GetTaggedEntitiesByTag(TaggableObjectTypes objectType, string tag, string tagGroup = null)
        {
            var sql = new Sql()
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
                .Where<TagDto>(dto => dto.Tag == tag);

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
                ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>(sql));
        }

        private IEnumerable<TaggedEntity> CreateTaggedEntityCollection(IEnumerable<dynamic> dbResult)
        {
            var list = new List<TaggedEntity>();
            foreach (var node in dbResult.GroupBy(x => (int)x.nodeId))
            {
                var properties = new List<TaggedProperty>();
                foreach (var propertyType in node.GroupBy(x => new { id = (int)x.propertyTypeId, alias = (string)x.Alias }))
                {
                    var tags = propertyType.Select(x => new Tag((int)x.tagId, (string)x.tag, (string)x.group));
                    properties.Add(new TaggedProperty(propertyType.Key.id, propertyType.Key.alias, tags));
                }
                list.Add(new TaggedEntity(node.Key, properties));
            }
            return list;
        }

        public IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string group = null)
        {
            var sql = GetTagsQuerySelect(true);

            sql = ApplyRelationshipJoinToTagsQuery(sql);

            sql = sql
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId);

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
                .Where<TagRelationshipDto>(dto => dto.NodeId == contentId);

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
                .Where<TagRelationshipDto>(dto => dto.NodeId == contentId)
                .Where<PropertyTypeDto>(dto => dto.Alias == propertyTypeAlias);

            sql = ApplyGroupFilterToTagsQuery(sql, group);

            return ExecuteTagsQuery(sql);
        }

        private Sql GetTagsQuerySelect(bool withGrouping = false)
        {
            var sql = new Sql();

            if (withGrouping)
            {
                sql = sql.Select("cmsTags.Id, cmsTags.Tag, cmsTags." + SqlSyntax.GetQuotedColumnName("Group") + @", Count(*) NodeCount");
            }
            else
            {
                sql = sql.Select("DISTINCT cmsTags.*");
            }

            return sql;
        }

        private Sql ApplyRelationshipJoinToTagsQuery(Sql sql)
        {
            return sql
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id);
        }

        private Sql ApplyGroupFilterToTagsQuery(Sql sql, string group)
        {
            if (!group.IsNullOrWhiteSpace())
            {
                sql = sql.Where<TagDto>(dto => dto.Group == group);
            }

            return sql;
        }

        private Sql ApplyGroupByToTagsQuery(Sql sql)
        {
            return sql.GroupBy(new string[] { "cmsTags.Id", "cmsTags.Tag", "cmsTags." + SqlSyntax.GetQuotedColumnName("Group") + @"" });
        }

        private IEnumerable<ITag> ExecuteTagsQuery(Sql sql)
        {
            var factory = new TagFactory();

            return Database.Fetch<TagDto>(sql).Select(factory.BuildEntity);
        }

        /// <summary>
        /// Assigns the given tags to a content item's property
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeId"></param>
        /// <param name="tags">The tags to assign</param>
        /// <param name="replaceTags">
        /// If set to true, this will replace all tags with the given tags, 
        /// if false this will append the tags that already exist for the content item
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// This can also be used to remove all tags from a property by specifying replaceTags = true and an empty tag list.
        /// </remarks>
        //public void AssignTagsToProperty(int contentId, string propertyTypeAlias, IEnumerable<ITag> tags, bool replaceTags)
        public void AssignTagsToProperty(int contentId, int propertyTypeId, IEnumerable<ITag> tags, bool replaceTags)
        {
            //First we need to ensure there are no duplicates
            var asArray = tags.Distinct(new TagComparer()).ToArray();

            //we don't have to do anything if there are no tags assigning and we're not replacing them
            if (asArray.Length == 0 && replaceTags == false)
            {
                return;
            }

            //next check if we're removing all of the tags
            if (asArray.Length == 0 && replaceTags)
            {
                Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId=" + contentId + " AND propertyTypeId=" + propertyTypeId);
                return;
            }

            //ok so now we're actually assigning tags...
            //NOTE: There's some very clever logic in the umbraco.cms.businesslogic.Tags.Tag to insert tags where they don't exist, 
            // and assign where they don't exist which we've borrowed here. The queries are pretty zany but work, otherwise we'll end up 
            // with quite a few additional queries.

            //do all this in one transaction
            using (var trans = Database.GetTransaction())
            {
                //var factory = new TagFactory();

                var tagSetSql = GetTagSet(asArray);

                //adds any tags found in the collection that aren't in cmsTag
                var insertTagsSql = string.Concat("insert into cmsTags (Tag,",
                                                  SqlSyntax.GetQuotedColumnName("Group"),
                                                  ") ",
                                                  " select TagSet.Tag, TagSet.",
                                                  SqlSyntax.GetQuotedColumnName("Group"),
                                                  " from ",
                                                  tagSetSql,
                                                  " left outer join cmsTags on (TagSet.Tag = cmsTags.Tag and TagSet.",
                                                  SqlSyntax.GetQuotedColumnName("Group"),
                                                  " = cmsTags.",
                                                  SqlSyntax.GetQuotedColumnName("Group"),
                                                  ")",
                                                  " where cmsTags.Id is null ");
                //insert the tags that don't exist
                Database.Execute(insertTagsSql);

                if (replaceTags)
                {
                    //if we are replacing the tags then remove them first
                    Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId=" + contentId + " AND propertyTypeId=" + propertyTypeId);
                }

                //adds any tags found in csv that aren't in tagrelationships
                var insertTagRelationsSql = string.Concat("insert into cmsTagRelationship (tagId,nodeId,propertyTypeId) ",
                                                          "select NewTagsSet.Id, " + contentId + ", " + propertyTypeId + " from  ",
                                                          "( ",
                                                          "select NewTags.Id from  ",
                                                          tagSetSql,
                                                          " inner join cmsTags as NewTags on (TagSet.Tag = NewTags.Tag and TagSet.",
                                                          SqlSyntax.GetQuotedColumnName("Group"),
                                                          " = TagSet.",
                                                          SqlSyntax.GetQuotedColumnName("Group"),
                                                          ") ",
                                                          ") as NewTagsSet ",
                                                          "left outer join cmsTagRelationship ",
                                                          "on (cmsTagRelationship.TagId = NewTagsSet.Id and cmsTagRelationship.nodeId = ",
                                                          contentId,
                                                          " and cmsTagRelationship.propertyTypeId = ",
                                                          propertyTypeId,
                                                          ") ",
                                                          "where cmsTagRelationship.tagId is null ");

                //insert the tags relations that don't exist
                Database.Execute(insertTagRelationsSql);

                //GO!
                trans.Complete();
            }
        }

        /// <summary>
        /// Removes any of the given tags from the property association
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeId"></param>
        /// <param name="tags">The tags to remove from the property</param>
        public void RemoveTagsFromProperty(int contentId, int propertyTypeId, IEnumerable<ITag> tags)
        {
            var tagSetSql = GetTagSet(tags);

            var deleteSql = string.Concat("DELETE FROM cmsTagRelationship WHERE nodeId = ",
                                          contentId,
                                          " AND propertyTypeId = ",
                                          propertyTypeId,
                                          " AND tagId IN ",
                                          "(SELECT id FROM cmsTags INNER JOIN ",
                                          tagSetSql,
                                          " ON (TagSet.Tag = cmsTags.Tag and TagSet." + SqlSyntax.GetQuotedColumnName("Group") + @" = cmsTags." + SqlSyntax.GetQuotedColumnName("Group") + @"))");

            Database.Execute(deleteSql);
        }

        public void ClearTagsFromProperty(int contentId, int propertyTypeId)
        {
            Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId AND propertyTypeId = @propertyTypeId", 
                new { nodeId = contentId, propertyTypeId = propertyTypeId });
        }

        public void ClearTagsFromEntity(int contentId)
        {
            Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId",
                new { nodeId = contentId });
        }

        private Guid GetNodeObjectType(TaggableObjectTypes type)
        {
            switch (type)
            {
                case TaggableObjectTypes.Content:
                    return new Guid(Constants.ObjectTypes.Document);
                case TaggableObjectTypes.Media:
                    return new Guid(Constants.ObjectTypes.Media);
                case TaggableObjectTypes.Member:
                    return new Guid(Constants.ObjectTypes.Member);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// This is a clever way to produce an SQL statement like this:
        /// 
        ///     (select 'Spacesdd' as Tag, 'default' as [Group] 
        ///     union 
        ///     select 'Cool' as Tag, 'default' as [Group]
        ///     ) as TagSet
        /// 
        /// This allows us to use the tags to be inserted as a temporary in memory table. 
        /// </summary>
        /// <param name="tagsToInsert"></param>
        /// <returns></returns>
        private string GetTagSet(IEnumerable<ITag> tagsToInsert)
        {
            //TODO: Fix this query, since this is going to be basically a unique query each time, this will cause some mem usage in peta poco,
            // and surely there's a nicer way!
            //TODO: When we fix, be sure to remove the @ symbol escape

            var array = tagsToInsert
                .Select(tag =>
                    string.Format("select '{0}' as Tag, '{1}' as " + SqlSyntax.GetQuotedColumnName("Group") + @"",
                        PetaPocoExtensions.EscapeAtSymbols(tag.Text.Replace("'", "''")), tag.Group))
                .ToArray();
            return "(" + string.Join(" union ", array).Replace("  ", " ") + ") as TagSet";
        }

        private class TagComparer : IEqualityComparer<ITag>
        {
            public bool Equals(ITag x, ITag y)
            {
                return x.Text == y.Text && x.Group == y.Group;
            }

            public int GetHashCode(ITag obj)
            {
                unchecked
                {
                    return (obj.Text.GetHashCode() * 397) ^ obj.Group.GetHashCode();
                }
            }
        }

    }
}