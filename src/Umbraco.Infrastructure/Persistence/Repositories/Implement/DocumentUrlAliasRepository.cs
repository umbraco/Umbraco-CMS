using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
internal class DocumentUrlAliasRepository : IDocumentUrlAliasRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlAliasRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">The scope accessor.</param>
    public DocumentUrlAliasRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    /// <inheritdoc/>
    public void Save(IEnumerable<PublishedDocumentUrlAlias> aliases)
    {
        IEnumerable<Guid> documentKeys = aliases.Select(x => x.DocumentKey).Distinct();

        var dtoDictionary = aliases
            .Select(BuildDto)
            .ToDictionary(x => (x.UniqueId, x.LanguageId, x.Alias));

        var toDelete = new List<int>();
        var toInsert = dtoDictionary.Values.ToDictionary(x => (x.UniqueId, x.LanguageId, x.Alias));

        foreach (IEnumerable<Guid> group in documentKeys.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Sql<ISqlContext> sql = Database.SqlContext.Sql()
                .Select<DocumentUrlAliasDto>()
                .From<DocumentUrlAliasDto>()
                .Where<DocumentUrlAliasDto>(x => group.Contains(x.UniqueId))
                .ForUpdate();

            List<DocumentUrlAliasDto> existingAliasesInBatch = Database.Fetch<DocumentUrlAliasDto>(sql);

            foreach (DocumentUrlAliasDto existing in existingAliasesInBatch)
            {
                if (dtoDictionary.TryGetValue((existing.UniqueId, existing.LanguageId, existing.Alias), out DocumentUrlAliasDto? found))
                {
                    found.Id = existing.Id;

                    // If we found it, we know we should not insert it as a new record.
                    toInsert.Remove((found.UniqueId, found.LanguageId, found.Alias));
                }
                else
                {
                    toDelete.Add(existing.Id);
                }
            }
        }

        // do the deletes and inserts
        if (toDelete.Count > 0)
        {
            Database.DeleteMany<DocumentUrlAliasDto>().Where(x => toDelete.Contains(x.Id)).Execute();
        }

        Database.InsertBulk(toInsert.Values);
    }

    /// <inheritdoc/>
    public IEnumerable<PublishedDocumentUrlAlias> GetAll()
    {
        List<DocumentUrlAliasDto>? dtos = Database.Fetch<DocumentUrlAliasDto>(
            Database.SqlContext.Sql().Select<DocumentUrlAliasDto>().From<DocumentUrlAliasDto>());

        return dtos.Select(BuildModel);
    }

    /// <inheritdoc/>
    public void DeleteByDocumentKey(IEnumerable<Guid> documentKeys)
    {
        foreach (IEnumerable<Guid> group in documentKeys.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Database.Execute(Database.SqlContext.Sql()
                .Delete<DocumentUrlAliasDto>()
                .WhereIn<DocumentUrlAliasDto>(x => x.UniqueId, group));
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This gets all document aliases directly from property data (using an optimized SQL query).
    /// This is more efficient than loading all IContent objects.
    /// </remarks>
    public IEnumerable<DocumentUrlAliasRaw> GetAllDocumentUrlAliases()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Append($"SELECT n.{QuotedColName("uniqueId")} AS {QuotedColName("DocumentKey")}, pd.{QuotedColName("languageId")}")
            .Append($", COALESCE(pd.{QuotedColName("textValue")}, pd.{QuotedColName("varcharValue")}) AS {QuotedColName("AliasValue")}")
            .From<PropertyDataDto>("pd")
            .InnerJoin<PropertyTypeDto>("pt").On<PropertyDataDto, PropertyTypeDto>((pd, pt) => pd.PropertyTypeId == pt.Id, "pd", "pt")
            .InnerJoin<ContentVersionDto>("cv").On<PropertyDataDto, ContentVersionDto>((pd, cv) => pd.VersionId == cv.Id, "pd", "cv")
            .InnerJoin<NodeDto>("n").On<ContentVersionDto, NodeDto>((cv, n) => cv.NodeId == n.NodeId, "cv", "n")
            .Where<PropertyTypeDto>(pt => pt.Alias == Constants.Conventions.Content.UrlAlias, "pt")
            .Where<ContentVersionDto>(cv => cv.Current == true, "cv")
            .Where<NodeDto>(n => n.Trashed == false, "n")
            .Where<NodeDto>(n => n.NodeObjectType == Constants.ObjectTypes.Document, "n") // Exclude blueprints
            .Append($"AND (pd.{QuotedColName("textValue")} IS NOT NULL OR pd.{QuotedColName("varcharValue")} IS NOT NULL)");

        return Database.Fetch<DocumentUrlAliasRaw>(sql);
    }

    private string QuotedColName(string name) => Database.SqlContext.SqlSyntax.GetQuotedColumnName(name);
    private PublishedDocumentUrlAlias BuildModel(DocumentUrlAliasDto dto) =>
        new()
        {
            Alias = dto.Alias,
            DocumentKey = dto.UniqueId,
            LanguageId = dto.LanguageId,
        };

    private DocumentUrlAliasDto BuildDto(PublishedDocumentUrlAlias model) =>
        new()
        {
            Alias = model.Alias,
            UniqueId = model.DocumentKey,
            LanguageId = model.LanguageId,
        };
}
