using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookRepository : IReadWriteQueryRepository<int, Webhook>
{
}
