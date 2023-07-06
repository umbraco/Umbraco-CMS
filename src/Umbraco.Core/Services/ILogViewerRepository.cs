using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ILogViewerRepository
{
    PagedModel<ILogEntry> GetLogs(LogTimePeriod logTimePeriod, int skip, int take);
}
