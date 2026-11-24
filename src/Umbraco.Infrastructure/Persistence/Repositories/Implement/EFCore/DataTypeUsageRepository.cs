using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <inheritdoc />
internal sealed class DataTypeUsageRepository : IDataTypeUsageRepository
{
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeUsageRepository" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    public DataTypeUsageRepository(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor)
        => _scopeAccessor = scopeAccessor;

    private IEFCoreScope<UmbracoDbContext> AmbientScope
        => _scopeAccessor.AmbientScope
           ?? throw new InvalidOperationException("A scope is required to query the database");

    /// <inheritdoc />
    public Task<bool> HasSavedValuesAsync(Guid dataTypeKey, CancellationToken cancellationToken)
        => AmbientScope.ExecuteWithContextAsync(db => db.PropertyTypes
            .Join(db.PropertyData, propertyType => propertyType.Id, propertyData => propertyData.PropertyTypeId, (propertyType, propertyData) => propertyType)
            .Join(db.Nodes, propertyType => propertyType.DataTypeId, node => node.NodeId, (propertyType, node) => node)
            .AnyAsync(node => node.UniqueId == dataTypeKey, cancellationToken));
}
