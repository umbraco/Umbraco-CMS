using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebHookService
{
    Task<Webhook> CreateAsync(Webhook webhook);

    Task UpdateAsync(Webhook updateModel);

    Task DeleteAsync(Guid key);

    Task<Webhook?> GetAsync(Guid key);

    Task<PagedModel<Webhook>> GetAllAsync(int skip, int take);

    Task<IEnumerable<Webhook>> GetByEventNameAsync(string eventName);
}
