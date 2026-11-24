using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <inheritdoc />
/// <summary>
///     Answers whether any value has been saved against a property type, and whether a content type exists.
/// </summary>
internal sealed class PropertyTypeUsageRepository : IPropertyTypeUsageRepository
{
    private static readonly List<Guid> _contentTypeObjectTypes =
    [
        Constants.ObjectTypes.DocumentType, Constants.ObjectTypes.MediaType, Constants.ObjectTypes.MemberType,
    ];

    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyTypeUsageRepository" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    public PropertyTypeUsageRepository(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor)
        => _scopeAccessor = scopeAccessor;

    private IEFCoreScope<UmbracoDbContext> AmbientScope
        => _scopeAccessor.AmbientScope
           ?? throw new InvalidOperationException("A scope is required to query the database");

    /// <inheritdoc />
    public Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias, CancellationToken cancellationToken)
        => AmbientScope.ExecuteWithContextAsync(db => db.PropertyTypes
            .Where(propertyType => propertyType.Alias == propertyAlias)
            .Join(db.PropertyData, propertyType => propertyType.Id, propertyData => propertyData.PropertyTypeId, (propertyType, propertyData) => propertyType)
            .Join(db.Nodes, propertyType => propertyType.ContentTypeId, node => node.NodeId, (propertyType, node) => node)
            .AnyAsync(node => node.UniqueId == contentTypeKey, cancellationToken));

    /// <inheritdoc />
    public Task<bool> ContentTypeExistAsync(Guid contentTypeKey, CancellationToken cancellationToken)
        => AmbientScope.ExecuteWithContextAsync(db => db.Nodes
            .Where(node => node.UniqueId == contentTypeKey && node.NodeObjectType != null)
            .AnyAsync(node => _contentTypeObjectTypes.Contains(node.NodeObjectType!.Value), cancellationToken));
}
