using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class TagsRepository : PetaPocoRepositoryBase<int, ITag>, ITagsRepository
    {
        protected TagsRepository(IDatabaseUnitOfWork work)
            : this(work, RuntimeCacheProvider.Current)
        {
        }

        internal TagsRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
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
            if (ids.Any())
            {
                return PerformGetAllOnIds(ids);
            }

            var sql = GetBaseQuery(false);

            return ConvertFromDtos(Database.Fetch<TagDto>(sql))
                .ToArray();// we don't want to re-iterate again!
        }

        private IEnumerable<ITag> PerformGetAllOnIds(params int[] ids)
        {
            if (ids.Any() == false) yield break;
            foreach (var id in ids)
            {
                yield return Get(id);
            }
        }

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

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
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

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ITag entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new TagFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        //TODO: Consider caching implications.

        public IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string group = null)
        {
            var nodeObjectType = GetNodeObjectType(objectType);

            var sql = new Sql()
                .Select("DISTINCT cmsTags.*")
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<ContentDto>()
                .On<ContentDto, TagRelationshipDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);

            if (group.IsNullOrWhiteSpace() == false)
            {
                sql = sql.Where<TagDto>(dto => dto.Group == group);
            }

            var factory = new TagFactory();

            return Database.Fetch<TagDto>(sql).Select(factory.BuildEntity);
        }

        public IEnumerable<ITag> GetTagsForEntity(int contentId, string group = null)
        {
            var sql = new Sql()
                .Select("DISTINCT cmsTags.*")
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .Where<TagRelationshipDto>(dto => dto.NodeId == contentId);

            if (group.IsNullOrWhiteSpace() == false)
            {
                sql = sql.Where<TagDto>(dto => dto.Group == group);
            }

            var factory = new TagFactory();

            return Database.Fetch<TagDto>(sql).Select(factory.BuildEntity);
        }

        public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string group = null)
        {
            var sql = new Sql()
                .Select("DISTINCT cmsTags.*")
                .From<TagDto>()
                .InnerJoin<TagRelationshipDto>()
                .On<TagRelationshipDto, TagDto>(left => left.TagId, right => right.Id)
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, TagRelationshipDto>(left => left.Id, right => right.PropertyTypeId)
                .Where<TagRelationshipDto>(dto => dto.NodeId == contentId)
                .Where<PropertyTypeDto>(dto => dto.Alias == propertyTypeAlias);

            if (group.IsNullOrWhiteSpace() == false)
            {
                sql = sql.Where<TagDto>(dto => dto.Group == group);
            }  

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
                                                  SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
                                                  ") ",
                                                  " select TagSet.Tag, TagSet.",
                                                  SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
                                                  " from ",
                                                  tagSetSql,
                                                  " left outer join cmsTags on (TagSet.Tag = cmsTags.Tag and TagSet.",
                                                  SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
                                                  " = cmsTags.",
                                                  SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
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
                                                          SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
                                                          " = TagSet.",
                                                          SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("Group"),
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
                                          " ON (TagSet.Tag = cmsTags.Tag and TagSet.[Group] = cmsTags.[Group]))");

            Database.Execute(deleteSql);
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
        private static string GetTagSet(IEnumerable<ITag> tagsToInsert)
        {
            var array = tagsToInsert.Select(tag => string.Format("select '{0}' as Tag, '{1}' as [Group]", tag.Text.Replace("'", "''"), tag.Group)).ToArray();
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