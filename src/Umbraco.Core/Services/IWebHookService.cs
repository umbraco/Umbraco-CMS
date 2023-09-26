using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebHookService
{
    Task<Webhook> CreateAsync(Webhook webhook);
    Task UpdateAsync(Webhook updateModel);

    Task DeleteAsync(Guid key);

    Task<Webhook?> GetAsync(Guid key);

    Task<IEnumerable<Webhook>> GetMultipleAsync(IEnumerable<Guid> keys);

    Task<IEnumerable<Webhook>> GetAllAsync();

    Task<IEnumerable<Webhook>> GetByEventName(string eventName);
}
