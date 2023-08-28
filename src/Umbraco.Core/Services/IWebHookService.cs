using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebHookService
{
    Task CreateAsync(Webhook webhook);

    Task Delete(Webhook webhook);

    Task<Webhook> GetAsync(Guid key);

    Task<Webhook> GetMultipleAsync(IEnumerable<Guid> keys);
}
