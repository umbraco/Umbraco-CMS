using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc cref="IRepositoryCacheVersionRepository" />
internal class RepositoryCacheVersionRepository : RepositoryBase, IRepositoryCacheVersionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryCacheVersionRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="appCaches">The application-level caches used for caching repository data.</param>
    public RepositoryCacheVersionRepository(IScopeAccessor scopeAccessor, AppCaches appCaches)
        : base(scopeAccessor, appCaches)
    {
    }

    /// <inheritdoc/>
    public async Task<RepositoryCacheVersion?> GetAsync(string identifier)
    {
        var version = await Database.ExecuteScalarAsync<string?>(GetQuery(identifier));
        return new RepositoryCacheVersion { Identifier = identifier, Version = version };
    }

    /// <inheritdoc/>
    public RepositoryCacheVersion? Get(string identifier)
    {
        var version = Database.ExecuteScalar<string?>(GetQuery(identifier));
        return new RepositoryCacheVersion { Identifier = identifier, Version = version };
    }

    /// <summary>
    /// Asynchronously retrieves all <see cref="RepositoryCacheVersion"/> instances from the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of <see cref="RepositoryCacheVersion"/>.</returns>
    public async Task<IEnumerable<RepositoryCacheVersion>> GetAllAsync()
    {
        IEnumerable<RepositoryCacheVersionDto> dtos = await Database.FetchAsync<RepositoryCacheVersionDto>(GetAllQuery());
        return dtos.Select(Map).Where(x => x is not null)!;
    }

    /// <inheritdoc/>
    public IEnumerable<RepositoryCacheVersion> GetAll()
    {
        IEnumerable<RepositoryCacheVersionDto> dtos = Database.Fetch<RepositoryCacheVersionDto>(GetAllQuery());
        return dtos.Select(Map).Where(x => x is not null)!;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(RepositoryCacheVersion repositoryCacheVersion)
        => await Database.InsertOrUpdateAsync(Map(repositoryCacheVersion), null, null);

    /// <inheritdoc/>
    public void Save(RepositoryCacheVersion repositoryCacheVersion)
        => Database.InsertOrUpdate(Map(repositoryCacheVersion), null, null);

    private Sql<ISqlContext> GetQuery(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be null or whitespace.", nameof(identifier));
        }

        return Sql()
            .Select<RepositoryCacheVersionDto>(x => x.Version)
            .From<RepositoryCacheVersionDto>()
            .Where<RepositoryCacheVersionDto>(x => x.Identifier == identifier);
    }

    private Sql<ISqlContext> GetAllQuery()
        => Sql()
            .Select<RepositoryCacheVersionDto>()
            .From<RepositoryCacheVersionDto>();

    private static RepositoryCacheVersionDto Map(RepositoryCacheVersion entity)
        => new() { Identifier = entity.Identifier, Version = entity.Version };

    private static RepositoryCacheVersion? Map(RepositoryCacheVersionDto? dto)
        => dto is null ? null : new RepositoryCacheVersion { Identifier = dto.Identifier, Version = dto.Version };
}
