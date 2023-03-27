using NPoco;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

// This could be made better with caching and stuff, but it's really a stop gap measure
// So for now we'll just use the database to resolve the key/id every time.
internal sealed class UserIdKeyResolver : IUserIdKeyResolver
{
    private readonly IScopeProvider _scopeProvider;

    public UserIdKeyResolver(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public Task<int?> GetAsync(Guid key)
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
        ISqlContext sqlContext = scope.SqlContext;

        Sql<ISqlContext> query = sqlContext.Sql()
            .Select<UserDto>(x => x.Id)
            .Where<UserDto>(x => x.Key == key);


        return scope.Database.ExecuteScalarAsync<int?>(query);
    }

    public Task<Guid?> GetAsync(int id)
    {
        using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
        ISqlContext sqlContext = scope.SqlContext;

        Sql<ISqlContext> query = sqlContext.Sql()
            .Select<UserDto>(x => x.Key)
            .Where<UserDto>(x => x.Id == id);

        return scope.Database.ExecuteScalarAsync<Guid?>(query);
    }
}
