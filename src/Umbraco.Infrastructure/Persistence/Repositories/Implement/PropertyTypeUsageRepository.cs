using System;
using NPoco;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class PropertyTypeUsageRepository : IPropertyTypeUsageRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public PropertyTypeUsageRepository(IScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
    }

    public bool HasSavedPropertyValues(string propertyTypeAlias)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;

        if (database is null)
        {
            throw new InvalidOperationException("A scope is required to query the database");
        }

        Sql<ISqlContext> selectQuery = database.SqlContext.Sql()
            .SelectAll()
            .From<PropertyTypeDto>("m")
            .InnerJoin<PropertyDataDto>("p")
            .On<PropertyDataDto, PropertyTypeDto>((left, right) => left.PropertyTypeId == right.Id, "p", "m")
            .Where<PropertyTypeDto>(m => m.Alias == propertyTypeAlias, "m");

        Sql<ISqlContext> hasValuesQuery = database.SqlContext.Sql()
            .SelectAnyIfExists(selectQuery);

        return database.ExecuteScalar<bool>(hasValuesQuery);
    }
}
