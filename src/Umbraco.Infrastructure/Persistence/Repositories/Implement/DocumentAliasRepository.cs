using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
internal class DocumentAliasRepository : IDocumentAliasRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentAliasRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">The scope accessor. </param>
    public DocumentAliasRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

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
    public void Save(IEnumerable<PublishedDocumentAlias> aliases)
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
                .Select<DocumentAliasDto>()
                .From<DocumentAliasDto>()
                .Where<DocumentAliasDto>(x => group.Contains(x.UniqueId))
                .ForUpdate();

            List<DocumentAliasDto> existingAliasesInBatch = Database.Fetch<DocumentAliasDto>(sql);

            foreach (DocumentAliasDto existing in existingAliasesInBatch)
            {
                if (dtoDictionary.TryGetValue((existing.UniqueId, existing.LanguageId, existing.Alias), out DocumentAliasDto? found))
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
            Database.DeleteMany<DocumentAliasDto>().Where(x => toDelete.Contains(x.Id)).Execute();
        }

        Database.InsertBulk(toInsert.Values);
    }

    /// <inheritdoc/>
    public IEnumerable<PublishedDocumentAlias> GetAll()
    {
        List<DocumentAliasDto>? dtos = Database.Fetch<DocumentAliasDto>(
            Database.SqlContext.Sql().Select<DocumentAliasDto>().From<DocumentAliasDto>());

        return dtos.Select(BuildModel);
    }

    /// <inheritdoc/>
    public void DeleteByDocumentKey(IEnumerable<Guid> documentKeys)
    {
        foreach (IEnumerable<Guid> group in documentKeys.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Database.Execute(Database.SqlContext.Sql()
                .Delete<DocumentAliasDto>()
                .WhereIn<DocumentAliasDto>(x => x.UniqueId, group));
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This gets all document aliases directly from property data (using an optimized SQL query).
    /// This is more efficient than loading all IContent objects.
    /// </remarks>
    public IEnumerable<DocumentAliasRaw> GetAllDocumentAliases()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<NodeDto>("n", x => x.UniqueId)
            .AndSelect<PropertyDataDto>("pd", x => x.LanguageId)
            .Append(", COALESCE(pd.textValue, pd.varcharValue) AS AliasValue")
            .From<PropertyDataDto>("pd")
            .InnerJoin<PropertyTypeDto>("pt").On<PropertyDataDto, PropertyTypeDto>((pd, pt) => pd.PropertyTypeId == pt.Id, "pd", "pt")
            .InnerJoin<ContentVersionDto>("cv").On<PropertyDataDto, ContentVersionDto>((pd, cv) => pd.VersionId == cv.Id, "pd", "cv")
            .InnerJoin<NodeDto>("n").On<ContentVersionDto, NodeDto>((cv, n) => cv.NodeId == n.NodeId, "cv", "n")
            .Where<PropertyTypeDto>(pt => pt.Alias == Constants.Conventions.Content.UrlAlias, "pt")
            .Where<ContentVersionDto>(cv => cv.Current == true, "cv")
            .Where<NodeDto>(n => n.Trashed == false, "n")
            .Where<NodeDto>(n => n.NodeObjectType == Constants.ObjectTypes.Document, "n") // Exclude blueprints
            .Append("AND (pd.textValue IS NOT NULL OR pd.varcharValue IS NOT NULL)");

        return Database.Fetch<DocumentAliasRaw>(sql);
    }

    private PublishedDocumentAlias BuildModel(DocumentAliasDto dto) =>
        new()
        {
            Alias = dto.Alias,
            DocumentKey = dto.UniqueId,
            LanguageId = dto.LanguageId,
            RootAncestorKey = dto.RootAncestorKey,
        };

    private DocumentAliasDto BuildDto(PublishedDocumentAlias model) =>
        new()
        {
            Alias = model.Alias,
            UniqueId = model.DocumentKey,
            LanguageId = model.LanguageId,
            RootAncestorKey = model.RootAncestorKey,
        };
}
