using Serilog.Events;

namespace Umbraco.Cms.Core.Logging.Viewer;

public interface ILogFilter
{
    bool TakeLogEvent(LogEvent e);
}
