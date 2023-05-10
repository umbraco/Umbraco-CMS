using System.Text;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class TagRepository : EntityRepositoryBase<int, ITag>, ITagRepository
{
    public TagRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<TagRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    #region Manage Tag Entities

    /// <inheritdoc />
    protected override ITag? PerformGet(int id)
    {
        Sql<ISqlContext> sql = Sql().Select<TagDto>().From<TagDto>().Where<TagDto>(x => x.Id == id);
        TagDto? dto = Database.Fetch<TagDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
        return dto == null ? null : TagFactory.BuildEntity(dto);
    }

    /// <inheritdoc />
    protected override IEnumerable<ITag> PerformGetAll(params int[]? ids)
    {
        IEnumerable<TagDto> dtos = ids?.Length == 0
            ? Database.Fetch<TagDto>(Sql().Select<TagDto>().From<TagDto>())
            : Database.FetchByGroups<TagDto, int>(ids!, Constants.Sql.MaxParameterCount,
                batch => Sql().Select<TagDto>().From<TagDto>().WhereIn<TagDto>(x => x.Id, batch));

        return dtos.Select(TagFactory.BuildEntity).ToList();
    }

    /// <inheritdoc />
    protected override IEnumerable<ITag> PerformGetByQuery(IQuery<ITag> query)
    {
        Sql<ISqlContext> sql = Sql().Select<TagDto>().From<TagDto>();
        var translator = new SqlTranslator<ITag>(sql, query);
        sql = translator.Translate();

        return Database.Fetch<TagDto>(sql).Select(TagFactory.BuildEntity).ToList();
    }

    /// <inheritdoc />
    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        isCount ? Sql().SelectCount().From<TagDto>() : GetBaseQuery();

    private Sql<ISqlContext> GetBaseQuery() => Sql().Select<TagDto>().From<TagDto>();

    /// <inheritdoc />
    protected override string GetBaseWhereClause() => "id = @id";

    /// <inheritdoc />
    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM cmsTagRelationship WHERE tagId = @id", "DELETE FROM cmsTags WHERE id = @id"
        };
        return list;
    }

    /// <inheritdoc />
    protected override void PersistNewItem(ITag entity)
    {
        entity.AddingEntity();

        TagDto dto = TagFactory.BuildDto(entity);
        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override void PersistUpdatedItem(ITag entity)
    {
        entity.UpdatingEntity();

        TagDto dto = TagFactory.BuildDto(entity);
        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    #endregion

    #region Assign and Remove Tags

    /// <inheritdoc />
    // only invoked from ContentRepositoryBase with all cultures + replaceTags being true
    public void Assign(int contentId, int propertyTypeId, IEnumerable<ITag> tags, bool replaceTags = true)
    {
        // to no-duplicates array
        ITag[] tagsA = tags.Distinct(new TagComparer()).ToArray();

        // replacing = clear all
        if (replaceTags)
        {
            Sql<ISqlContext> sql0 = Sql().Delete<TagRelationshipDto>()
                .Where<TagRelationshipDto>(x => x.NodeId == contentId && x.PropertyTypeId == propertyTypeId);
            Database.Execute(sql0);
        }

        // no tags? nothing else to do
        if (tagsA.Length == 0)
        {
            return;
        }

        // tags
        // using some clever logic (?) to insert tags that don't exist in 1 query
        // must coalesce languageId because equality of NULLs does not exist

        var tagSetSql = GetTagSet(tagsA);
        var group = SqlSyntax.GetQuotedColumnName("group");

        // insert tags
        var sql1 = $@"INSERT INTO cmsTags (tag, {group}, languageId)
SELECT tagSet.tag, tagSet.{group}, tagSet.languageId
FROM {tagSetSql}
LEFT OUTER JOIN cmsTags ON (tagSet.tag = cmsTags.tag AND tagSet.{group} = cmsTags.{group} AND COALESCE(tagSet.languageId, -1) = COALESCE(cmsTags.languageId, -1))
WHERE cmsTags.id IS NULL";

        Database.Execute(sql1);

        // insert relations
        var sql2 = $@"INSERT INTO cmsTagRelationship (nodeId, propertyTypeId, tagId)
SELECT {contentId}, {propertyTypeId}, tagSet2.Id
FROM (
    SELECT t.Id
    FROM {tagSetSql}
    INNER JOIN cmsTags as t ON (tagSet.tag = t.tag AND tagSet.{group} = t.{group} AND COALESCE(tagSet.languageId, -1) = COALESCE(t.languageId, -1))
) AS tagSet2
LEFT OUTER JOIN cmsTagRelationship r ON (tagSet2.id = r.tagId AND r.nodeId = {contentId} AND r.propertyTypeID = {propertyTypeId})
WHERE r.tagId IS NULL";

        Database.Execute(sql2);
    }

    /// <inheritdoc />
    // only invoked from tests
    public void Remove(int contentId, int propertyTypeId, IEnumerable<ITag> tags)
    {
        var tagSetSql = GetTagSet(tags);
        var group = SqlSyntax.GetQuotedColumnName("group");

        var deleteSql =
            $@"DELETE FROM cmsTagRelationship WHERE nodeId = {contentId} AND propertyTypeId = {propertyTypeId} AND tagId IN (
                            SELECT id FROM cmsTags INNER JOIN {tagSetSql} ON (
                                    tagSet.tag = cmsTags.tag AND tagSet.{group} = cmsTags.{group} AND COALESCE(tagSet.languageId, -1) = COALESCE(cmsTags.languageId, -1)
                                )
                            )";

        Database.Execute(deleteSql);
    }

    /// <inheritdoc />
    public void RemoveAll(int contentId, int propertyTypeId) =>
        Database.Execute(
            "DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId AND propertyTypeId = @propertyTypeId",
            new {nodeId = contentId, propertyTypeId});

    /// <inheritdoc />
    public void RemoveAll(int contentId) =>
        Database.Execute("DELETE FROM cmsTagRelationship WHERE nodeId = @nodeId",
            new {nodeId = contentId});

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
        var sql = new StringBuilder();
        var group = SqlSyntax.GetQuotedColumnName("group");
        var first = true;

        sql.Append("(");

        foreach (ITag tag in tags)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sql.Append(" UNION ");
            }

            // HACK: SQLite (or rather SQL server setup was a hack)
            if (SqlContext.DatabaseType.IsSqlServer())
            {
                sql.Append("SELECT N'");
            }
            else
            {
                sql.Append("SELECT '");
            }

            sql.Append(SqlSyntax.EscapeString(tag.Text));
            sql.Append("' AS tag, '");
            sql.Append(SqlSyntax.EscapeString(tag.Group));
            sql.Append("' AS ");
            sql.Append(group);
            sql.Append(" , ");
            if (tag.LanguageId.HasValue)
            {
                sql.Append(tag.LanguageId);
            }
            else
            {
                sql.Append("NULL");
            }

            sql.Append(" AS languageId");
        }

        sql.Append(") AS tagSet");

        return sql.ToString();
    }

    // used to run Distinct() on tags
    private class TagComparer : IEqualityComparer<ITag>
    {
        public bool Equals(ITag? x, ITag? y) =>
            ReferenceEquals(x, y) // takes care of both being null
            || (x != null && y != null && x.Text == y.Text && x.Group == y.Group && x.LanguageId == y.LanguageId);

        public int GetHashCode(ITag obj)
        {
            unchecked
            {
                var h = obj.Text.GetHashCode();
                h = (h * 397) ^ obj.Group.GetHashCode();
                h = (h * 397) ^ (obj.LanguageId?.GetHashCode() ?? 0);
                return h;
            }
        }
    }

    #endregion

    #region Queries

    // TODO: consider caching implications
    // add lookups for parentId or path (ie get content in tag group, that are descendants of x)

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private class TaggedEntityDto
    {
        public int NodeId { get; set; }
        public string? PropertyTypeAlias { get; set; }
        public int PropertyTypeId { get; set; }
        public int TagId { get; set; }
        public string TagText { get; set; } = null!;
        public string TagGroup { get; set; } = null!;
        public int? TagLanguage { get; set; }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Local

    /// <inheritdoc />
    public TaggedEntity? GetTaggedEntityByKey(Guid key)
    {
        Sql<ISqlContext> sql = GetTaggedEntitiesSql(TaggableObjectTypes.All, "*");

        sql = sql
            .Where<NodeDto>(dto => dto.UniqueId == key);

        return Map(Database.Fetch<TaggedEntityDto>(sql)).FirstOrDefault();
    }

    /// <inheritdoc />
    public TaggedEntity? GetTaggedEntityById(int id)
    {
        Sql<ISqlContext> sql = GetTaggedEntitiesSql(TaggableObjectTypes.All, "*");

        sql = sql
            .Where<NodeDto>(dto => dto.NodeId == id);

        return Map(Database.Fetch<TaggedEntityDto>(sql)).FirstOrDefault();
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedEntitiesByTagGroup(TaggableObjectTypes objectType, string group,
        string? culture = null)
    {
        Sql<ISqlContext> sql = GetTaggedEntitiesSql(objectType, culture);

        sql = sql
            .Where<TagDto>(x => x.Group == group);

        return Map(Database.Fetch<TaggedEntityDto>(sql));
    }

    /// <inheritdoc />
    public IEnumerable<TaggedEntity> GetTaggedEntitiesByTag(TaggableObjectTypes objectType, string tag,
        string? group = null, string? culture = null)
    {
        Sql<ISqlContext> sql = GetTaggedEntitiesSql(objectType, culture);

        sql = sql
            .Where<TagDto>(dto => dto.Text == tag);

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        return Map(Database.Fetch<TaggedEntityDto>(sql));
    }

    private Sql<ISqlContext> GetTaggedEntitiesSql(TaggableObjectTypes objectType, string? culture)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<TagRelationshipDto>(x => Alias(x.NodeId, "NodeId"))
            .AndSelect<PropertyTypeDto>(x => Alias(x.Alias, "PropertyTypeAlias"),
                x => Alias(x.Id, "PropertyTypeId"))
            .AndSelect<TagDto>(x => Alias(x.Id, "TagId"), x => Alias(x.Text, "TagText"),
                x => Alias(x.Group, "TagGroup"), x => Alias(x.LanguageId, "TagLanguage"))
            .From<TagDto>()
            .InnerJoin<TagRelationshipDto>().On<TagDto, TagRelationshipDto>((tag, rel) => tag.Id == rel.TagId)
            .InnerJoin<ContentDto>()
            .On<TagRelationshipDto, ContentDto>((rel, content) => rel.NodeId == content.NodeId)
            .InnerJoin<PropertyTypeDto>()
            .On<TagRelationshipDto, PropertyTypeDto>((rel, prop) => rel.PropertyTypeId == prop.Id)
            .InnerJoin<NodeDto>().On<ContentDto, NodeDto>((content, node) => content.NodeId == node.NodeId);

        if (culture == null)
        {
            sql = sql
                .Where<TagDto>(dto => dto.LanguageId == null);
        }
        else if (culture != "*")
        {
            sql = sql
                .InnerJoin<LanguageDto>().On<TagDto, LanguageDto>((tag, lang) => tag.LanguageId == lang.Id)
                .Where<LanguageDto>(x => x.IsoCode == culture);
        }

        if (objectType != TaggableObjectTypes.All)
        {
            Guid nodeObjectType = GetNodeObjectType(objectType);
            sql = sql.Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);
        }

        return sql;
    }

    private static IEnumerable<TaggedEntity> Map(IEnumerable<TaggedEntityDto> dtos) =>
        dtos.GroupBy(x => x.NodeId).Select(dtosForNode =>
        {
            var taggedProperties = dtosForNode.GroupBy(x => x.PropertyTypeId).Select(dtosForProperty =>
            {
                string? propertyTypeAlias = null;
                var tags = dtosForProperty.Select(dto =>
                {
                    propertyTypeAlias = dto.PropertyTypeAlias;
                    return new Tag(dto.TagId, dto.TagGroup, dto.TagText, dto.TagLanguage);
                }).ToList();
                return new TaggedProperty(dtosForProperty.Key, propertyTypeAlias, tags);
            }).ToList();

            return new TaggedEntity(dtosForNode.Key, taggedProperties);
        }).ToList();

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string? group = null,
        string? culture = null)
    {
        Sql<ISqlContext> sql = GetTagsSql(culture, true);

        AddTagsSqlWhere(sql, culture);

        if (objectType != TaggableObjectTypes.All)
        {
            Guid nodeObjectType = GetNodeObjectType(objectType);
            sql = sql
                .Where<NodeDto>(dto => dto.NodeObjectType == nodeObjectType);
        }

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        sql = sql
            .GroupBy<TagDto>(x => x.Id, x => x.Text, x => x.Group, x => x.LanguageId);

        return ExecuteTagsQuery(sql);
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForEntity(int contentId, string? group = null, string? culture = null)
    {
        Sql<ISqlContext> sql = GetTagsSql(culture);

        AddTagsSqlWhere(sql, culture);

        sql = sql
            .Where<NodeDto>(dto => dto.NodeId == contentId);

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        return ExecuteTagsQuery(sql);
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForEntity(Guid contentId, string? group = null, string? culture = null)
    {
        Sql<ISqlContext> sql = GetTagsSql(culture);

        AddTagsSqlWhere(sql, culture);

        sql = sql
            .Where<NodeDto>(dto => dto.UniqueId == contentId);

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        return ExecuteTagsQuery(sql);
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null,
        string? culture = null)
    {
        Sql<ISqlContext> sql = GetTagsSql(culture);

        sql = sql
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyTypeDto, TagRelationshipDto>((prop, rel) => prop.Id == rel.PropertyTypeId)
            .Where<NodeDto>(x => x.NodeId == contentId)
            .Where<PropertyTypeDto>(x => x.Alias == propertyTypeAlias);

        AddTagsSqlWhere(sql, culture);

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        return ExecuteTagsQuery(sql);
    }

    /// <inheritdoc />
    public IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string? group = null,
        string? culture = null)
    {
        Sql<ISqlContext> sql = GetTagsSql(culture);

        sql = sql
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyTypeDto, TagRelationshipDto>((prop, rel) => prop.Id == rel.PropertyTypeId)
            .Where<NodeDto>(dto => dto.UniqueId == contentId)
            .Where<PropertyTypeDto>(dto => dto.Alias == propertyTypeAlias);

        AddTagsSqlWhere(sql, culture);

        if (group.IsNullOrWhiteSpace() == false)
        {
            sql = sql
                .Where<TagDto>(dto => dto.Group == group);
        }

        return ExecuteTagsQuery(sql);
    }

    private Sql<ISqlContext> GetTagsSql(string? culture, bool withGrouping = false)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<TagDto>();

        if (withGrouping)
        {
            sql = sql
                .AndSelectCount("NodeCount");
        }

        sql = sql
            .From<TagDto>()
            .InnerJoin<TagRelationshipDto>().On<TagRelationshipDto, TagDto>((rel, tag) => tag.Id == rel.TagId)
            .InnerJoin<ContentDto>()
            .On<ContentDto, TagRelationshipDto>((content, rel) => content.NodeId == rel.NodeId)
            .InnerJoin<NodeDto>().On<NodeDto, ContentDto>((node, content) => node.NodeId == content.NodeId);

        if (culture != null && culture != "*")
        {
            sql = sql
                .InnerJoin<LanguageDto>().On<TagDto, LanguageDto>((tag, lang) => tag.LanguageId == lang.Id);
        }

        return sql;
    }

    private Sql<ISqlContext> AddTagsSqlWhere(Sql<ISqlContext> sql, string? culture)
    {
        if (culture == null)
        {
            sql = sql
                .Where<TagDto>(dto => dto.LanguageId == null);
        }
        else if (culture != "*")
        {
            sql = sql
                .Where<LanguageDto>(x => x.IsoCode == culture);
        }

        return sql;
    }

    private IEnumerable<ITag> ExecuteTagsQuery(Sql sql) =>
        Database.Fetch<TagDto>(sql).Select(TagFactory.BuildEntity);

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
