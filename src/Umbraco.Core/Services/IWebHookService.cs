using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebHookService
{
    Task CreateAsync(Webhook webhook);

    Task DeleteAsync(Webhook webhook);

    Task<Webhook?> GetAsync(Guid key);

    Task<IEnumerable<Webhook>> GetMultipleAsync(IEnumerable<Guid> keys);
}
