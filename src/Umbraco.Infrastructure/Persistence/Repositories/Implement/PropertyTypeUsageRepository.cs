using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc/>
internal sealed class PropertyTypeUsageRepository : IPropertyTypeUsageRepository
{
    private static readonly List<Guid> _nodeObjectTypes =
    [
        Constants.ObjectTypes.DocumentType, Constants.ObjectTypes.MediaType, Constants.ObjectTypes.MemberType,
    ];

    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyTypeUsageRepository"/> class.
    /// </summary>
    public PropertyTypeUsageRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <inheritdoc/>
    public Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database
            ?? throw new InvalidOperationException("A scope is required to query the database");

        Sql<ISqlContext> selectQuery = database.SqlContext.Sql()
            .SelectAll()
            .From<PropertyTypeDto>("pt")
            .InnerJoin<PropertyDataDto>("pd")
            .On<PropertyDataDto, PropertyTypeDto>((pd, pt) => pd.PropertyTypeId == pt.Id, "pd", "pt")
            .InnerJoin<NodeDto>("n")
            .On<PropertyTypeDto, NodeDto>((pt, n) => pt.ContentTypeId == n.NodeId, "pt", "n")
            .Where<PropertyTypeDto>(pt => pt.Alias == propertyAlias, "pt")
            .Where<NodeDto>(n => n.UniqueId == contentTypeKey, "n");

        Sql<ISqlContext> hasValuesQuery = database.SqlContext.Sql()
            .SelectAnyIfExists(selectQuery);

        return Task.FromResult(database.ExecuteScalar<bool>(hasValuesQuery));
    }

    /// <inheritdoc/>
    public Task<bool> ContentTypeExistAsync(Guid contentTypeKey)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database
            ?? throw new InvalidOperationException("A scope is required to query the database");

        Sql<ISqlContext> selectQuery = database.SqlContext.Sql()
            .SelectAll()
            .From<NodeDto>("n")
            .Where<NodeDto>(n => n.UniqueId == contentTypeKey, "n")
            .WhereIn<NodeDto>(n => n.NodeObjectType, _nodeObjectTypes, "n");

        Sql<ISqlContext> hasValuesQuery = database.SqlContext.Sql()
            .SelectAnyIfExists(selectQuery);

        return Task.FromResult(database.ExecuteScalar<bool>(hasValuesQuery));
    }
}
