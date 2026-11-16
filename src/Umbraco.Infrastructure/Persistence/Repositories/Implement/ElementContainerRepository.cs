using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents a repository for managing element containers (folders) in the persistence layer.
/// </summary>
internal sealed class ElementContainerRepository : EntityContainerRepository, IElementContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for caching entities and query results.</param>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public ElementContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<ElementContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.ElementContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    protected override void PersistDeletedItem(EntityContainer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Element containers can be referenced as start nodes on individual users (umbracoUserStartNode)
        // and on user groups (umbracoUserGroup.startElementId). Both reference umbracoNode.id via FK,
        // so we must clear those references before deleting the underlying node.
        var args = new { id = entity.Id };
        Database.Execute(
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.UserStartNode)} WHERE {QuoteColumnName("startNode")} = @id",
            args);
        Database.Execute(
            $@"UPDATE {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup)}
               SET {QuoteColumnName("startElementId")} = NULL
               WHERE {QuoteColumnName("startElementId")} = @id",
            args);

        // delete all granular permissions for this element container
        Database.Delete<UserGroup2GranularPermissionDto>(Sql().Where<UserGroup2GranularPermissionDto>(dto => dto.UniqueId == entity.Key));

        base.PersistDeletedItem(entity);
    }
}
