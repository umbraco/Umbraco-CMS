using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ILogViewerQueryRepository : IReadWriteQueryRepository<int, ILogViewerQuery>
{
    ILogViewerQuery? GetByName(string name);
}
