using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface ILogViewerQueryRepository : IAsyncReadWriteQueryRepository<int, ILogViewerQuery>
    {
        ILogViewerQuery? GetByName(string name);
    }
}
