namespace Umbraco.Cms.Core.Services;

public interface IUserIdKeyResolver
{
    public Task<int?> GetAsync(Guid key);

    public Task<Guid?> GetAsync(int id);
}
